<main>
  <slot />
</main>

<style>
  @import url("https://fonts.googleapis.com/css2?family=Work+Sans:wght@200;400;900&display=swap");
  @import url("https://fonts.googleapis.com/css2?family=Cutive+Mono&display=swap");

  :root {
    --color: #9f9f9f;
  }

  :global(body, html) {
    margin: 0;
    padding: 0;
  }
  :global(*) {
    font-family: 'Helvetica Neue', system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, 'Open Sans', sans-serif;
    font-weight: 200;
  }

  :global(body) {
    height: 100vh;
  }

  :global(button) {
    border: none;
    border-radius: .25rem;
    background: #5b81bb;
    color: white;
    text-transform: uppercase;
    padding: 1rem;
  }

  main {
    margin: 1rem;
    display: flex;
    flex-direction: column;
    flex-grow: 1;
  }
</style>
