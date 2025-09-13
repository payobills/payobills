<script lang="ts">
  export let title: string;
</script>

<div class="card">
  <div class="title">{title}</div>
</div>

<style>
  
  .card {
    width: calc(100% - 2 * 1rem);
    background-color: rgb(151, 151, 151);
    aspect-ratio: 1.8 / 1;
    border-radius: 0.25rem;
    box-shadow: 0rem 1rem 1rem rgb(105, 105, 105);
    margin: 1rem 0;
    padding: 1rem;
    display: flex;
  }

  .title {
    font-family: "Figtree", -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', 'Helvetica Neue', sans-serif;
    color: white;
    font-weight: 400;
    font-size: 1.2rem;
    text-transform: uppercase;
    align-self: flex-end;
  }
</style>
