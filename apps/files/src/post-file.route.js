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

        const corelationIdHeaderName = 'X-Corelation-ID'
        const corelationID = req.headers[corelationIdHeaderName.toLowerCase()]

        if(!corelationID) {
            res.status(400).json({ detail: `Missing ${corelationIdHeaderName} header` });
            return
        }

        const tags = {
            corelationID,
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
