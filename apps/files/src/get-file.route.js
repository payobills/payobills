const fs = require('fs')
const { Duplex } = require('stream')
const axios = require('axios')
const { EVENT_TYPE__NEW_FILE } = require('./constants')

/**
 * @param {{
 * minioClient: import("minio").Client
 * }}
 */
const getFile = ({ nocoDbClient }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        const fileId = req.params['id'];
        console.log('id', req.params['id'])

        const projectId = process.env.NOCODB_PROJECT_ID || "payobills";
        const tableId = process.env.NOCODB_FILES_TABLE_ID || "files";
        const fileRecord = await nocoDbClient.getRecord(
            projectId,
            tableId,
            fileId
        );

        const signedNocodbPath = fileRecord.Files?.[0]?.signedPath || null;

        if (signedNocodbPath === null) {
            res.status(404).send({ error: 'File not found' });
            return;
        }

        const mimeType = fileRecord.Files?.[0]?.mimeType || 'application/pdf';
        const fileExtension = `.${mimeType.split('/').pop()}` || '';
        const url = `${nocoDbClient.getBaseUrl()}/${signedNocodbPath}`
        const fileStream = await axios.get(url, { responseType: "stream" });

        const generatedFileName = `payobills-file-${fileId}${fileExtension}`;

        res.header('Content-Disposition', `attachment; filename="${generatedFileName}"`);
        res.header('Content-Type', mimeType);
        res.header('Content-Length', fileStream.headers['content-length']);

        fileStream.data.pipe(res)

    }
}

module.exports = { getFile }
