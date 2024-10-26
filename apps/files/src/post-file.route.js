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

        const correlationId = tagsToAdd[correlationIdPropertyKeyFromPayload]

        // Add all tags, and only the CorrelationId with the right casing
        const tags = {
            ...tagsToAdd,
            [correlationIdPropertyKeyFromPayload]: undefined,
            [correlationIdPropertyKey]: correlationId,
        }

        await nocoDbClient.putObject(
            null,
            req.file.originalname,
            req.file.buffer,
            tags
        )

        const message = { type: EVENT_TYPE__NEW_FILE, args: { correlationId } }
        const messageString = JSON.stringify(message);
        rabbitChannel.sendToQueue('payobills.files', Buffer.from(messageString));

        // TODO: Return link to get by ID for the file as header as specified by REST Spec
        res.status(201).end()
    }
}

module.exports = { postFile }
