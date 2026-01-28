/// @ts-check
import { ApolloServer } from '@apollo/server';
import { expressMiddleware } from '@as-integrations/express5';
import {
  ApolloServerPluginDrainHttpServer,
} from '@apollo/server/plugin/drainHttpServer';
import { printSchema } from 'graphql';
import { buildSubgraphSchema } from '@apollo/subgraph';
import Express from 'express';
import http from 'node:http';
import { typeDefs as scalarTypeDefs } from 'graphql-scalars';
import * as graphqlTag from 'graphql-tag';

import {
  ApolloServerPluginInlineTraceDisabled,
} from '@apollo/server/plugin/disabled';

// @ts-ignore
import 'graphql-import-node/register.js';

// import NocoDbClientFactory from './nocodb-client.factory';

async function main() {
  let app = Express();
  let port = +(process.env.PORT || 80);
  let host = process.env.HOST || '0.0.0.0';

  // const nocoDbClient = await NocoDbClientFactory.generate();
  // const DI = {
  //   nocoDbClient,
  // };

  const typeDefs = graphqlTag.default`
    scalar DateTimeISO

    type Query {
      me: MeResponse!
    }

    extend schema
      @link(
        url: "https://specs.apollo.dev/federation/v2.0"
        import: ["@key", "@shareable"]
      )

    """
    User Entity to represent a user
    """
    type User @key(fields: "id") {
      """
      Unique ID for a user
      """
      id: ID!
      createdAt: DateTimeISO!
      updatedAt: DateTimeISO!
    }

    """
    Response type for a user querying about their details
    """
    type MeResponse {
      user: User!
    }
  `;

  const mockUser = {
    id: '1',
    createdAt: new Date(),
    updatedAt: new Date(),
  };

  const resolvers = {
    Query: {
      me() {
        return {
          user: {
            id: '1',
            createdAt: new Date(),
            updatedAt: new Date(),
          },
        };
      },
    },
    User: {
      __resolveReference() {
        return mockUser;
      },
    },
  };

  const httpServer = http.createServer(app);

  /// @ts-ignore
  const schema = buildSubgraphSchema({ scalarTypeDefs, typeDefs, resolvers })

  // Set up Apollo Server
  const apolloServer = new ApolloServer({
    schema,
    csrfPrevention: false,
    plugins: [
      ApolloServerPluginDrainHttpServer({ httpServer }),
      ApolloServerPluginInlineTraceDisabled(),
    ],
  });

  await apolloServer.start();

  app.use(
    '/graphql',
    Express.json(),
    /// @ts-ignore
    expressMiddleware(apolloServer),
  );

  /// @ts-ignore
  app.get('/sdl', (_, res) => {
    res.setHeader('Content-Type', 'text/plain');
    res.send(printSchema(schema));
  });

  /// @ts-ignore
  app.get('/', (_, res) => res.send({ app: 'user-service' }));

  console.log(`[INFO] App listening on port ${host}:${port}`);
  await new Promise((resolve) => httpServer.listen({ port, host, resolve }));
}

try {
  await main();
}
catch (e) { console.error(e) }

export { };
