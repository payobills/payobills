const NocoDbClient = require("./nocodb-client")

module.exports = {
    generate: () => {
        return new NocoDbClient({
            baseUrl: process.env.NOCO_DB_BASE_URL,
            xcToken: process.env.NOCO_DB_XC_TOKEN,
            projectId: process.env.NOCO_DB_PROJECT_ID,
            tableId: process.env.NOCO_DB_TABLE_ID
        });
    }
}