const { Duplex } = require('stream')
const uuid = require('uuid')

/**
 * @param {{
 * minioClient: import("minio").Client
 * }}
 */
const postFile = ({ nocoDbClient }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {

        const correlationIdHeaderName = 'X-Correlation-ID'
        const correlationID = req.headers[correlationIdHeaderName.toLowerCase()]

        if(!correlationID) {
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

        res.status(201).end()
    }
}

module.exports = { postFile }
