/// @ts-check
const { ApolloServer } = require('@apollo/server');
const { expressMiddleware } = require('@as-integrations/express5');
const { ApolloServerPluginDrainHttpServer } = require('@apollo/server/plugin/drainHttpServer');
const { printSchema } = require('graphql');
const { buildSubgraphSchema } = require('@apollo/subgraph');
const Express = require("express");
const multer = require('multer');
const http = require('http');
const { typeDefs: scalarTypeDefs }= require('graphql-scalars')

const NocoDbClientFactory = require('./nocodb-client.factory');
const rabbitmqChannelFactory = require("./rabbitmq-channel.factory");

const { postFile } = require("./post-file.route");
const { default: gql } = require('graphql-tag');

const fileResolver = require('./resolvers/file');

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

  const typeDefs = gql`
  scalar DateTimeISO

  extend schema
    @link(
      url: "https://specs.apollo.dev/federation/v2.0"
      import: ["@key", "@shareable"]
    )

  type File
    @key(fields: "id") {
    id: ID!
    downloadPath: String
    fileName: String
    createdAt: DateTimeISO!
    updatedAt: DateTimeISO
  }
`;

  const resolvers = {
    File: {
      __resolveReference(file, _) {
        return fileResolver.getFile(DI)(file);
      },
    },
  };

  const httpServer = http.createServer(app);

  // Set up Apollo Server
  const apolloServer = new ApolloServer({
    /// @ts-ignore
    schema: buildSubgraphSchema({ scalarTypeDefs, typeDefs, resolvers }),
    csrfPrevention: false,
    // typeDefs,
    // resolvers,
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

  console.log(`App listening on port ${host}:${port}`)
  await new Promise((resolve) => httpServer.listen({ port, host, resolve }));
}

main().catch(console.error)
