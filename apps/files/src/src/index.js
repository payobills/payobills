/// @ts-check
const Express = require("express");
const multer = require('multer');

const MinioClientFactory = require('./minio-client.factory');

const { postFile, patchFile, getFile } = require("./files.route");

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);
  let host = (process.env.HOST || 'localhost');

  const upload = multer()

  var minioClient = MinioClientFactory.generate();

  const DI = {
    minioClient
  };

  app.get('/', (_, res) => (res.send({ app: 'files service' })))
  /// @ts-ignore
  app.post('/files', upload.single('file'), postFile(DI))
  app.get('/files/:fileId([a-z0-9A-Z-/]*)', getFile(DI))
  app.patch('/files/:fileId([a-z0-9A-Z-/]*)', patchFile(DI))
  app.listen(port, host, () => {
    console.log(`app listening at ${host}:${port}`)
  })
}

main().catch(console.error)
