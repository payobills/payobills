const { Duplex } = require('stream')
const uuid = require('uuid')

/**
 * @param {{
 * minioClient: import("minio").Client
 * }}
 */
const postFile = ({minioClient }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        let rawFile = req.file;

        const correlationIdHeaderName='X-Payobills-Correlation-ID'
        const correlationId = req.headers[correlationIdHeaderName.toLowerCase()]

        const fileId = uuid.v4()
        const fileExtension = req.file.mimetype.split('/')[1]
        const fileName = `${fileId}.${fileExtension}`

        const tags = {
            [correlationIdHeaderName]: correlationId,
            mimeType: req.file.mimetype
        }
        const createdFile = await minioClient.putObject(
            process.env.STORAGE__BUCKET_NAME,
            fileName,
            req.file.buffer,
            { 'Content-Type': tags.mimeType }
        )

        await minioClient.setObjectTagging(
            process.env.STORAGE__BUCKET_NAME,
            fileName,
            tags
        )

        res.status(201).send({
            data: {
                ...createdFile,
                tags,
            }
        })
    }
}

module.exports = { postFile }
