import { createClient } from "@urql/svelte";
import { readable } from "svelte/store";

const client = createClient({
  url: '/bills-graphql/graphql'
});

export const urql = readable(client)
