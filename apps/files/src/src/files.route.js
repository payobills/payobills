const { Duplex } = require('stream')
const uuid = require('uuid')

/**
 * @param {{
 * minioClient: import("minio").Client
 * }}
 */
const postFile = ({minioClient }) => {
    /**
     * @param {import('express').Request} req
     * @param {import('express').Response} res
     */
    return async (req, res) => {
        const correlationIdHeaderName='X-Payobills-Correlation-ID'
        const correlationId = req.headers[correlationIdHeaderName.toLowerCase()]

        const fileId = uuid.v4()
        const fileExtension = req.file.mimetype.split('/')[1]
        const fileName = `${process.env.STORAGE__TMP_UPLOAD_PREFIX}/${fileId}.${fileExtension}`

        const tags = {
            [correlationIdHeaderName]: correlationId,
            mimeType: req.file.mimetype
        }
        const createdFile = await minioClient.putObject(
            process.env.STORAGE__BUCKET_NAME,
            fileName,
            req.file.buffer,
            { 'Content-Type': tags.mimeType }
        )

        await minioClient.setObjectTagging(
            process.env.STORAGE__BUCKET_NAME,
            fileName,
            tags
        )

        res.status(201).send({
            data: {
                ...createdFile,
                tags,
            }
        })
    }
}

/**
 * @param {{
* minioClient: import("minio").Client
* }}
*/
const getFile = ({ minioClient }) => {
   /**
    * @param {import('express').Request} req
    * @param {import('express').Response} res
    */
   return async (req, res) => {
       const {fileId} = req.params

    //    console.log({params:req.params})
       // const fileExtension = req.file.mimetype.split('/')[1]
       // const fileName = `${fileId}.${fileExtension}`


    // /** @type Promise<minioClient.Tag[]> */
       const tagMappings = await minioClient.getObjectTagging(
           process.env.STORAGE__BUCKET_NAME,
           fileId
       )

       //    tagMappings.map()
       
       const tags = tagMappings[0].reduce((acc, curr) => {
         const newAcc = {...acc, [curr.Key]: curr.Value}
         return newAcc
       }, {})
    //    console.log(JSON.stringify(tagMappings[0]))
    //    console.log(JSON.stringify(tags))

       res.status(200).send({
           data: { tags }
       })
   }
}

/**
 * @param {{
* minioClient: import("minio").Client
* }}
*/
const patchFile = ({ minioClient }) => {
   /**
    * @param {import('express').Request} req
    * @param {import('express').Response} res
    */
   return async (req, res) => {
    //    const correlationIdHeaderName='X-Payobills-Correlation-ID'
    //    const correlationId = req.headers[correlationIdHeaderName.toLowerCase()]

       const {fileId} = req.params

       console.log({fileId})

       const sourceObjectName = `${process.env.STORAGE__BUCKET_NAME}/tmp-uploads/${fileId}`
       const destObjectName = `bills/${fileId}`
       
       await minioClient.copyObject(
           process.env.STORAGE__BUCKET_NAME,
           destObjectName,
           sourceObjectName,
       )

       await minioClient.setObjectTagging(
           process.env.STORAGE__BUCKET_NAME,
           destObjectName,
           tags
       )

       res.status(200).send({
           data: {
               tags,
           }
       })
   }
}

module.exports = { postFile, getFile, patchFile }
