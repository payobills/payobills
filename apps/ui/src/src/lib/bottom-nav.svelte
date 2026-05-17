<script lang="ts">
import { goto } from "$app/navigation";
import { page } from "$app/stores";
import IconButton from "$lib/icon-button.svelte";
import {
  faHouse,
  faPlus,
  faFileInvoiceDollar,
  faListUl,
} from "@fortawesome/free-solid-svg-icons";

const navItems = [
  { label: "Home", icon: faHouse, href: "/timeline" },
  { label: "Txns", icon: faListUl, href: "/transactions" },
  { label: "Bills", icon: faFileInvoiceDollar, href: "/bills" },
  { label: "Add Txn", icon: faPlus, href: "/transaction/add" },
  { label: "Add Bill", icon: faPlus, href: "/bills/add" },
];

$: currentPath = $page.url.pathname;
</script>

<nav class="bottom-nav">
  {#each navItems as item}
    {@const isActive = currentPath === item.href}
    <button
      class="nav-item"
      class:active={isActive}
      on:click={() => goto(item.href)}
    >
      <IconButton
        icon={item.icon}
        scale={0.8}
        color="var(--color-neutral-content)"
        backgroundColor="transparent"
      />
      <span class="nav-label">{item.label}</span>
    </button>
  {/each}
</nav>

<style>
  .bottom-nav {
    position: fixed;
    bottom: 0;
    left: 0;
    right: 0;
    height: 4.5rem;
    background-color: var(--color-base-200);
    border-top: 1px solid var(--color-base-300);
    display: flex;
    align-items: center;
    justify-content: space-around;
    padding-bottom: env(safe-area-inset-bottom);
    z-index: 100;
  }

  .nav-item {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.2rem;
    background: transparent;
    border: none;
    cursor: pointer;
    padding: 0.5rem 1rem;
    flex: 1;
  }

  .nav-label {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.6rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: var(--color-neutral-content);
  }

  .nav-item.active .nav-label {
    color: var(--color-primary);
  }

  .nav-item.active :global(.icon-button svg) {
    fill: var(--color-primary) !important;
  }

  @media (min-width: 72rem) {
    .bottom-nav {
      top: 50%;
      bottom: auto;
      left: 0;
      right: auto;
      transform: translateY(-50%);
      width: 5rem;
      height: auto;
      flex-direction: column;
      justify-content: center;
      gap: 0.5rem;
      border-top: none;
      border-right: 1px solid var(--color-base-300);
      border-radius: 0 1rem 1rem 0;
      padding: 1rem 0;
    }

    .nav-item {
      flex: none;
      width: 100%;
      padding: 0.75rem 0;
    }
  }
</style>
