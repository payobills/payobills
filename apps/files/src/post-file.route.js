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
        const correlationId = req.headers[correlationIdHeaderName.toLowerCase()]

        const tags = {
            originalName: req.file.originalname,
            mimeType: req.file.mimetype,
            correlationId: correlationId ? correlationId : undefined
        }

        await nocoDbClient.putObject(
            process.env.STORAGE__BUCKET_NAME,
            req.file.originalname,
            req.file.buffer,
            tags
        )

        res.status(201).send({
            data: {
                tags,
            }
        })
    }
}

module.exports = { postFile }
