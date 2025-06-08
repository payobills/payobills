module.exports = {
    getFile: ({ nocoDbClient }) => async (file) => {
        const projectId = process.env.NOCODB_PROJECT_ID || "payobills";
        const tableId = process.env.NOCODB_FILES_TABLE_ID || "files";

        const record = await nocoDbClient.getRecord(
            projectId,
            tableId,
            file.id
        );

        /**
         * @type {String} filePath is the path to the file in NocoDB.
         */
        const filePath = record.Files[0]?.path || "";
        const extension = (filePath.search(/\./) !== -1) ? filePath.split(".").pop() : ""

        return {
            id: record.Id,
            downloadPath: `/files/${file.id}`,
            fileName: record.Files?.[0]?.title || null,
            extension,
            mimeType: record.Files?.[0]?.mimetype,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt || null,
        };
    }
}
