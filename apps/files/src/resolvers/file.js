module.exports = {
    getFile: ({ nocoDbClient }) => async (file) => {
        const record = await nocoDbClient.getRecord(
            "payobills",
            "files",
            file.id
        );
        return {
            id: record.Id,
            downloadPath: record.Files[0].signedPath,
            fileName: record.Files[0].title,
            createdAt: record.CreatedAt,
            updatedAt: record.UpdatedAt || null,
        };
    }
}