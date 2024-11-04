const { Duplex } = require('stream')
const { EVENT_TYPE__NEW_FILE } = require('./constants')

/**
 * @param {{
 * minioClient: import("minio").Client
 * }}
 */
const postFile = ({ nocoDbClient, rabbitChannel }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        const correlationIdPropertyKey = 'CorrelationId'
        const tagsToAdd = JSON.parse(req.body?.tags || '{}');
        const correlationIdPropertyKeyLowered = correlationIdPropertyKey.toLowerCase();
        const correlationIdPropertyKeyFromPayload = Object.keys(tagsToAdd).find(p => p.toLowerCase() === correlationIdPropertyKeyLowered); 
    
        if (correlationIdPropertyKeyFromPayload === undefined) {
            res.status(400).json({ detail: `Missing ${correlationIdPropertyKey} property in tags` });
            return
        }

        const fileTypePropertyKey = 'Type'
        const fileTypePropertyKeyLowered = fileTypePropertyKey.toLowerCase();
        const fileTypePropertyKeyFromPayload = Object.keys(tagsToAdd).find(p => p.toLowerCase() === fileTypePropertyKeyLowered); 
    
        if (fileTypePropertyKeyFromPayload === undefined || tagsToAdd[fileTypePropertyKeyFromPayload].trim() === '') {
            res.status(400).json({ detail: `Missing ${fileTypePropertyKey} property in tags` });
            return
        }

        const correlationId = tagsToAdd[correlationIdPropertyKeyFromPayload]
        const fileType = tagsToAdd[fileTypePropertyKeyFromPayload]

        // Add all tags, and only the CorrelationId with the right casing
        const tags = {
            ...tagsToAdd,
            [correlationIdPropertyKeyFromPayload]: undefined,
            [correlationIdPropertyKey]: correlationId,
            [fileTypePropertyKeyFromPayload]: undefined,
            [fileTypePropertyKey]: fileType
        }

        let objectData = await nocoDbClient.putObject(
            null,
            req.file.originalname,
            req.file.buffer,
            tags
        )

        const message = { type: EVENT_TYPE__NEW_FILE, args: { id: objectData.etag, correlationId, type: fileType } }
        const messageString = JSON.stringify(message);
        rabbitChannel.sendToQueue('payobills.files', Buffer.from(messageString));

        // TODO: Return link to get by ID for the file as header as specified by REST Spec
        res.status(201).end()
    }
}

module.exports = { postFile }

