<slot />

<style>
  @import url("https://fonts.googleapis.com/css2?family=Work+Sans:wght@200;400;900&display=swap");
  @import url("https://fonts.googleapis.com/css2?family=Cutive+Mono&display=swap");

  :global(body, html) {
    margin: 0;
    padding: 0;
  }
  :global(*) {
    font-family: Quicksand, "Work Sans", sans-serif;
  }

  :global(body){
    height: 100vh;
  }
</style>

<script lang="ts">
  import { createClient, setContextClient } from '@urql/svelte';

  const client = createClient({
    url: `/graphql`,
  });

  setContextClient(client);
</script>
