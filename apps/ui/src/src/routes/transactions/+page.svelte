<script lang="ts">
  import { goto } from "$app/navigation";
  import { onMount } from "svelte";

  onMount(() => {
    const currentTransactionMonth = `transactions/${new Date().getUTCFullYear()}/${new Date().getMonth()}`;
    goto(currentTransactionMonth);
  });
</script>
