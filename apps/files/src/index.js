/// @ts-check
const Express = require("express");
const multer = require('multer');

const NocoDbClientFactory = require('./nocodb-client.factory');
const MongoClientFactory = require("./mongo-client.factory");
const { postFile } = require("./post-file.route");

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);
  let host = process.env.HOST || '0.0.0.0';

  const upload = multer()

  var nocoDbClient = NocoDbClientFactory.generate();

  const DI = {
    nocoDbClient
  };

  app.get('/', (_, res) => (res.send({ app: 'files-service' })))
  /// @ts-ignore
  app.post('/files', upload.single('file'), postFile(DI))
  app.listen(port, host, () => {
    console.log(`App listening on port ${port}`)
  })
}

main().catch(console.error)
