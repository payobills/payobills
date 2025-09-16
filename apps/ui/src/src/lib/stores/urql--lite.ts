import { offlineExchange } from "@urql/exchange-graphcache";
import { makeDefaultStorage } from "@urql/exchange-graphcache/default-storage";
import { createClient, fetchExchange } from "@urql/svelte";
import { readable } from "svelte/store";

export const getLiteUrql = () => {
  const introspectedSchema = {
    __schema: {
      queryType: { name: 'Query' },
      mutationType: { name: 'Mutation' },
      subscriptionType: { name: 'Subscription' },
    },
  };

  const request = window.indexedDB.open("graphcache-v3", 3);

  const storage = makeDefaultStorage({
    idbName: 'graphcache-v3', // The name of the IndexedDB database
    maxAge: 10000000000, // The maximum age of the persisted data in days
  });

  const cache = offlineExchange({
    schema: introspectedSchema,
    updates: {
      /* ... */
    },
    storage,
    optimistic: {
      /* ... */
    },
  });

  const liteClient = createClient({
    url: '/gateway/graphql',
    exchanges: [cache as any, fetchExchange]
  });

  return liteClient
}
