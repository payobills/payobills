const mongodb = require("mongodb");

module.exports = {
    generate: () => {
        const mongoClient = new mongodb.MongoClient(process.env["DATABASE_URL"] || "")
        return mongoClient
    }
}
