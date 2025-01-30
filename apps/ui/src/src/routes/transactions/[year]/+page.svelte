<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";

  onMount(() => {
    let path = window.location.pathname;
    const currentYear = +path.split("/")[2];
    const currentTransactionMonth = `/transactions/${currentYear}/${new Date().getUTCMonth() + 1}`;
    goto(currentTransactionMonth);
  });
</script>
