const mongodb = require("mongodb");

module.exports = {
    generate: () => {
        const mongoClient = new mongodb.MongoClient(process.env["DATABASE__URL"] || "")
        return mongoClient
    }
}
