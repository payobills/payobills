import { createClient } from "@urql/svelte";
import { readable } from "svelte/store";

const billsClient = createClient({
  url: '/bills-graphql/graphql'
});
export const billsUrql = readable(billsClient)

const paymentsClient = createClient({
  url: '/payments-graphql/graphql'
});
export const paymentsUrql = readable(paymentsClient)
