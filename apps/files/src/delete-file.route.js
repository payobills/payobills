const deleteFile = ({ nocoDbClient }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        const fileId = req.params['id'];
        const projectId = process.env.NOCODB_PROJECT_ID || "payobills";
        const tableId = process.env.NOCODB_FILES_TABLE_ID || "files";

        const originalFileRecord = await nocoDbClient.getRecord(
            projectId,
            tableId,
            fileId
        );

        if (originalFileRecord?.Tags?.IsMarkedDeleted) {
            res.status(404).send({ error: 'NOT_FOUND', message: 'File not found' });
            return;
        }

        const fileRecordWithUpdatedTags = {
            Tags: {
                ...originalFileRecord.Tags,
                TransactionID: undefined,
                BackupTransactionID: originalFileRecord.Tags.TransactionID,
                IsMarkedDeleted: true
            }
        }

        await nocoDbClient.patchRecord(
            fileRecordWithUpdatedTags,
            fileId
        );

        res.status(200).send({ message: 'File deleted', });
    }
}

module.exports = { deleteFile }
