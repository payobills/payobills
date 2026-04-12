<script lang="ts">
export let title: string;
</script>

<div class="card">
  <div class="title">{title}</div>
</div>

<style>
  .card {
    width: 100%;
    background: linear-gradient(135deg, #1c1c26 0%, #111118 60%, #0d0d1a 100%);
    border: 1px solid #2a2a38;
    aspect-ratio: 1.8 / 1;
    border-radius: 0.75rem;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255,255,255,0.04);
    margin: 0.75rem 0;
    padding: 1.25rem;
    display: flex;
    position: relative;
    overflow: hidden;
  }

  .card::before {
    content: '';
    position: absolute;
    top: -40%;
    right: -20%;
    width: 60%;
    height: 160%;
    background: radial-gradient(ellipse, rgba(0, 212, 184, 0.06) 0%, transparent 70%);
    pointer-events: none;
  }

  .title {
    font-family: "Syne", system-ui, sans-serif;
    color: var(--color-base-content, #dde1eb);
    font-weight: 600;
    font-size: 1rem;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    align-self: flex-end;
  }
</style>
