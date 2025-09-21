import { offlineExchange } from "@urql/exchange-graphcache";
import { makeDefaultStorage } from "@urql/exchange-graphcache/default-storage";
import { createClient, fetchExchange } from "@urql/svelte";
import { readable } from "svelte/store";
  import { queryStore, gql, getContextClient } from "@urql/svelte";

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
      Mutation: {
        addBill(result, _args, cache, _info) {
        const TodoList = gql`
          {
            todos {
              id
            }
          }
        `;

        cache.updateQuery({ query: TodoList }, data => {
          return {
            ...data,
            todos: [...data.todos, result.createTodo],
          };
        });
      },
      }
    },
    storage,
    optimistic: {
      /* ... */
    },
  });

  const liteClient = createClient({
    url: '/invalid-graphql-url',
    exchanges: [cache as any, fetchExchange]
  });

  return liteClient
}
