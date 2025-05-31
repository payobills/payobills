/// @ts-check
const { ApolloServer } = require('@apollo/server');
const { expressMiddleware } = require('@as-integrations/express5');
const { ApolloServerPluginDrainHttpServer } = require('@apollo/server/plugin/drainHttpServer');
const { buildSchema, printSchema } = require('graphql');
const Express = require("express");
const multer = require('multer');
const http = require('http');

const NocoDbClientFactory = require('./nocodb-client.factory');
const MongoClientFactory = require("./mongo-client.factory");
const rabbitmqChannelFactory = require("./rabbitmq-channel.factory");

const { postFile } = require("./post-file.route");

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);
  let host = process.env.HOST || '0.0.0.0';

  const upload = multer()

  var nocoDbClient = await NocoDbClientFactory.generate();
  var rabbitChannel = await rabbitmqChannelFactory.generate();

  const DI = {
    nocoDbClient,
    rabbitChannel
  };

  const typeDefs = `#graphql
  type Query {
    hello: String
  }
`;

  // A map of functions which return data for the schema.
  const resolvers = {
    Query: {
      hello: () => 'world',
    },
  };

  const httpServer = http.createServer(app);

  // Set up Apollo Server
  const apolloServer = new ApolloServer({
    csrfPrevention: false,
    typeDefs,
    resolvers,
    plugins: [
      ApolloServerPluginDrainHttpServer({ httpServer }),
      // ApolloServerPluginLandingPageDisabled()
      //  ApolloServerPluginLandingPageLocalDefault()
    ],
  });

  await apolloServer.start();

  app.use(
    '/graphql',
    Express.json(),
    /// @ts-ignore
    expressMiddleware(apolloServer),
  );

  app.get('/', (_, res) => (res.send({ app: 'files-service' })))
  /// @ts-ignore
  app.post('/files', upload.single('file'), postFile(DI))

  app.get('/graphql-sdl', (_, res) => {
    const schema = buildSchema(typeDefs);
    const sdlString = printSchema(schema);
    res.header('Content-Type', 'application/graphql+json');
    res.send(sdlString);
  });

  console.log(`App listening on port ${host}:${port}`)
  await new Promise((resolve) => httpServer.listen({ port, host, resolve }));
}

main().catch(console.error)
