<script lang="ts">
  import { onMount } from 'svelte';

  let tripId: string | undefined;
  let tripTitle: string | undefined;

  onMount(() => {
      tripId = new URLSearchParams(window.location.search)?.get('id');
      tripTitle = new URLSearchParams(window.location.search)?.get('title');
  });

</script>

<section>
  <h1>{tripTitle}</h1>

  TODO: Add transactions from this trip
</section>

