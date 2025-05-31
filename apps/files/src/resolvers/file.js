module.exports = {
    getFile: ({ nocoDbClient }) => async (file) => {
        const record = await nocoDbClient.getRecord(
            "payobills",
            "files",
            file.id
        );
        return {
            id: record.Id,
            downloadPath: record.Files?.[0]?.signedPath || null,
            fileName: record.Files?.[0]?.title || null,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt || null,
        };
    }
}