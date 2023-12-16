/// @ts-check
const Express = require("express");

const MongoClientFactory = require("./mongo-client.factory");
const { postFile } = require("./routes/post-file");

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);

  var mongoClient = MongoClientFactory.generate();
  await mongoClient.connect()

  const DI = {
    mongoClient
  };

  app.get('/', (_, res) => (res.send({ app: 'files service' })))
  /// @ts-ignore
  app.post('/files', postFile(DI))
  app.listen(port, () => {
    console.log(`app listening on port ${port}`)
  })
}

main().catch(console.error)
