/// @ts-check
const Express = require("express");
const multer = require('multer');

const MinioClientFactory = require('./minio-client.factory');
const MongoClientFactory = require("./mongo-client.factory");
const { postFile } = require("./post-file.route");

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);
  let host = +(process.env.HOST || 'localhost');

  const upload = multer()

  var minioClient = MinioClientFactory.generate();

  const DI = {
    minioClient
  };

  app.get('/', (_, res) => (res.send({ app: 'files service' })))
  /// @ts-ignore
  app.post('/files', upload.single('file'), postFile(DI))
  app.listen(port, '0.0.0.0', () => {
    console.log(`app listening on port ${port}`)
  })
}

main().catch(console.error)
