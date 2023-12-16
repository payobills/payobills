/**
 * @param {{mongoClient: import("mongodb").MongoClient}}
 */
const postFile = ({mongoClient}) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        let rawFile = req.file;

        let fileInput = {
            originalName: rawFile.originalname,
            key: rawFile.filename,
            pathPrefix: '/',
            mimeType: rawFile.mimetype,
            createdAt: new Date(),
            updatedAt: new Date()
        }

        const createdFile = await mongoClient
            .db("payobills-files")
            .collection("files")
            .insertOne(fileInput)

        res.status(201).send({
            data: createdFile
        })
    }
}

module.exports = { postFile }
