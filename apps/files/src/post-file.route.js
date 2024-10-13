const { Duplex } = require('stream')
const uuid = require('uuid')
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

        const correlationIdHeaderName = 'X-Correlation-ID'
        const correlationID = req.headers[correlationIdHeaderName.toLowerCase()]

        if (!correlationID) {
            res.status(400).json({ detail: `Missing ${correlationIdHeaderName} header` });
            return
        }

        const tags = {
            correlationID,
        }

        await nocoDbClient.putObject(
            null,
            req.file.originalname,
            req.file.buffer,
            tags
        )

        const message = { type: EVENT_TYPE__NEW_FILE, args: { correlationID } }
        const messageString = JSON.stringify(message);
        rabbitChannel.sendToQueue('payobills.files', Buffer.from(messageString));

        res.status(201).end()
    }
}

module.exports = { postFile }
