module.exports = {
    getFile: ({ nocoDbClient }) => async (file) => {
        const projectId = process.env.NOCODB_PROJECT_ID || "payobills";
        const tableId = process.env.NOCODB_FILES_TABLE_ID || "files";
        const record = await nocoDbClient.getRecord(
            projectId,
            tableId,
            file.id
        );
        return {
            id: record.Id,
            downloadPath: `/files/${file.id}`,
            fileName: record.Files?.[0]?.title || null,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt || null,
        };
    }
}
