import { createClient, setContextClient } from "@urql/svelte";
import { readable } from "svelte/store";

const client = createClient({
  url: `/graphql`,
});

export const urql = readable(client)
