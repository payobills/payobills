<script lang="ts">
import { goto } from "$app/navigation";
import { nav } from "$lib/stores/nav";
import { page } from "$app/stores";

let menuOpen = false;

const navLinks = [
  { label: "Timeline", href: "/lite/timeline" },
  { label: "Bills", href: "/lite/bills/add" },
  { label: "Transactions", href: "/lite/transactions" },
  { label: "Add Transaction", href: "/lite/transaction/add" },
];

function toggleMenu() {
  menuOpen = !menuOpen;
}

function navigateTo(href: string) {
  menuOpen = false;
  goto(href);
}

function handleOverlayClick() {
  menuOpen = false;
}
</script>

{#if $nav.isOpen}
<nav>
  <button class="app-title" on:click={() => goto($nav.link)}>
    <span class="logo-text">{$nav.title}</span>
  </button>

  <button class="hamburger" on:click={toggleMenu} aria-label="Open menu" aria-expanded={menuOpen}>
    <span class="bar" class:open={menuOpen}></span>
    <span class="bar" class:open={menuOpen}></span>
    <span class="bar" class:open={menuOpen}></span>
  </button>
</nav>

{#if menuOpen}
  <!-- svelte-ignore a11y-click-events-have-key-events -->
  <!-- svelte-ignore a11y-no-static-element-interactions -->
  <div class="overlay" on:click={handleOverlayClick}></div>
  <div class="menu-panel">
    <div class="menu-header">
      <span class="menu-title">Navigate</span>
      <button class="menu-close" on:click={toggleMenu} aria-label="Close menu">✕</button>
    </div>
    <ul class="menu-links">
      {#each navLinks as link}
        <li>
          <button
            class="menu-link"
            class:active={$page.url.pathname.startsWith(link.href)}
            on:click={() => navigateTo(link.href)}
          >
            <span class="link-label">{link.label}</span>
            <span class="link-arrow">→</span>
          </button>
        </li>
      {/each}
    </ul>
  </div>
{/if}
{/if}

<style>
  nav {
    background-color: var(--color-base-100, #09090e);
    border-bottom: 1px solid #1c1c26;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 1.25rem;
    margin: 0;
    height: 3.75rem;
    position: sticky;
    top: 0;
    z-index: 200;
    backdrop-filter: blur(12px);
  }

  .app-title {
    display: flex;
    align-items: center;
    font-size: 1.375rem;
    font-weight: 800;
    padding: 0;
    margin: 0;
    font-family: "Syne", system-ui, sans-serif;
    text-transform: none;
    letter-spacing: -0.03em;
    background-color: transparent;
    color: var(--color-base-content, #dde1eb);
    border: none;
    cursor: pointer;
  }

  .logo-text {
    font-family: "Syne";
    font-weight: 800;
    color: var(--color-base-content, #dde1eb);
  }

  /* Hamburger */
  .hamburger {
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 5px;
    background: transparent;
    border: 1px solid #2a2a38;
    border-radius: 0.375rem;
    padding: 0.5rem 0.6rem;
    cursor: pointer;
    width: 2.5rem;
    height: 2.5rem;
    flex-shrink: 0;
    transition: border-color 0.2s ease;
  }

  .hamburger:hover {
    border-color: rgba(0, 212, 184, 0.4);
  }

  .bar {
    display: block;
    width: 100%;
    height: 1.5px;
    background-color: var(--color-base-content, #dde1eb);
    border-radius: 1px;
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    transform-origin: center;
  }

  .bar.open:nth-child(1) {
    transform: translateY(6.5px) rotate(45deg);
  }
  .bar.open:nth-child(2) {
    opacity: 0;
    transform: scaleX(0);
  }
  .bar.open:nth-child(3) {
    transform: translateY(-6.5px) rotate(-45deg);
  }

  /* Overlay */
  .overlay {
    position: fixed;
    inset: 0;
    background-color: rgba(0, 0, 0, 0.55);
    backdrop-filter: blur(2px);
    z-index: 300;
  }

  /* Slide-in menu panel */
  .menu-panel {
    position: fixed;
    top: 0;
    right: 0;
    width: min(280px, 85vw);
    height: 100dvh;
    background-color: #111118;
    border-left: 1px solid #1c1c26;
    z-index: 400;
    display: flex;
    flex-direction: column;
    animation: slideIn 0.25s cubic-bezier(0.4, 0, 0.2, 1) forwards;
  }

  @keyframes slideIn {
    from { transform: translateX(100%); }
    to   { transform: translateX(0); }
  }

  .menu-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 1.25rem 1.25rem 1rem;
    border-bottom: 1px solid #1c1c26;
  }

  .menu-title {
    font-family: "Syne", sans-serif;
    font-size: 0.6875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: #4a5568;
  }

  .menu-close {
    background: transparent;
    border: 1px solid #2a2a38;
    border-radius: 0.375rem;
    color: var(--color-neutral-content, #8892a4);
    font-size: 0.75rem;
    width: 1.75rem;
    height: 1.75rem;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    padding: 0;
    transition: all 0.15s ease;
  }

  .menu-close:hover {
    border-color: rgba(0, 212, 184, 0.4);
    color: var(--color-primary, #00d4b8);
  }

  .menu-links {
    list-style: none;
    margin: 0;
    padding: 0.75rem 0;
    flex-grow: 1;
  }

  .menu-links li {
    padding: 0;
  }

  .menu-link {
    display: flex;
    align-items: center;
    justify-content: space-between;
    width: 100%;
    padding: 0.875rem 1.25rem;
    background: transparent;
    border: none;
    color: var(--color-base-content, #dde1eb);
    font-family: "Syne", sans-serif;
    font-size: 1rem;
    font-weight: 600;
    letter-spacing: -0.01em;
    text-align: left;
    cursor: pointer;
    transition: all 0.15s ease;
    border-left: 2px solid transparent;
    text-transform: none;
  }

  .menu-link:hover {
    background-color: rgba(28, 28, 38, 0.8);
    border-left-color: var(--color-primary, #00d4b8);
    color: var(--color-primary, #00d4b8);
  }

  .menu-link.active {
    background-color: rgba(0, 212, 184, 0.06);
    border-left-color: var(--color-primary, #00d4b8);
    color: var(--color-primary, #00d4b8);
  }

  .link-arrow {
    font-size: 0.875rem;
    color: #3a3a50;
    transition: color 0.15s ease;
  }

  .menu-link:hover .link-arrow,
  .menu-link.active .link-arrow {
    color: var(--color-primary, #00d4b8);
  }
</style>
