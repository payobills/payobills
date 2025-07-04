import { createClient, fetchExchange } from "@urql/svelte";
import { readable } from "svelte/store";

const billsClient = createClient({
  url: '/gateway/graphql',
  exchanges: [fetchExchange]
});
export const billsUrql = readable(billsClient)

const paymentsClient = createClient({
  url: '/gateway/graphql',
  exchanges:[fetchExchange]
});
export const paymentsUrql = readable(paymentsClient)
