<script lang="ts">
import IconButton from "$lib/icon-button.svelte";
import {
  faBell,
  faChartSimple,
  faListOl,
} from "@fortawesome/free-solid-svg-icons";

const iconStyle = (colorsLeftToRight: string[] = []) => `
    background: linear-gradient(to right, ${colorsLeftToRight.join(", ")});
    border: none;
    color: white;
    margin-right: 2rem;
    border-radius: 2.5rem;
    height: 5rem;
    width: 5rem;
    cursor: pointer;
  `;
</script>

<section>
  <div class="content">
    <div class="hero">
      <h1>Payobills <span class="accent">Lite</span></h1>
      <p class="one-liner">
        <span class="bold">payobills lite</span> keeps you on top of your billing
        and transaction needs — without sharing data with third parties.
        Your data, your control.
      </p>
    </div>

    <div class="features">
      <div class="feature-card">
        <div class="feature-icon" style="background: rgba(200, 80, 163, 0.12); border: 1px solid rgba(200,80,163,0.25);">
          <IconButton
            icon={faChartSimple}
            scale={0.6}
            style="background: transparent; border: none; color: #C850A3; width: 2.75rem; height: 2.75rem; margin: 0;"
          />
        </div>
        <div class="feature-text">
          <h3>Bill Management</h3>
          <p>Track all your bills together in a single place. No need to do it across different apps.</p>
        </div>
      </div>

      <div class="feature-card">
        <div class="feature-icon" style="background: rgba(97, 193, 199, 0.12); border: 1px solid rgba(97,193,199,0.25);">
          <IconButton
            icon={faBell}
            scale={0.6}
            style="background: transparent; border: none; color: #61C1C7; width: 2.75rem; height: 2.75rem; margin: 0;"
          />
        </div>
        <div class="feature-text">
          <h3>Reminders</h3>
          <p>Get notified about when to pay your bills, so you never miss a due date.</p>
        </div>
      </div>

      <div class="feature-card">
        <div class="feature-icon" style="background: rgba(240, 179, 71, 0.12); border: 1px solid rgba(240,179,71,0.25);">
          <IconButton
            icon={faListOl}
            scale={0.6}
            style="background: transparent; border: none; color: #F0B347; width: 2.75rem; height: 2.75rem; margin: 0;"
          />
        </div>
        <div class="feature-text">
          <h3>Transaction History</h3>
          <p>Clear, categorized view of your monthly spending so you know where your money goes.</p>
        </div>
      </div>
    </div>

    <button on:click={() => {}}>Coming Soon</button>

    <p class="footnote">
      * Payobills is open source, maintained by <span class="bold">Sahu, S</span> in personal time. ❤️ appreciated.
    </p>
  </div>
</section>

<style>
  :global(main) {
    align-items: center;
    justify-content: stretch;
  }

  section {
    padding: 1.5rem 1.25rem;
    align-items: stretch;
    display: flex;
    flex-direction: column;
    max-width: 36rem;
    width: 100%;
    flex-grow: 1;
  }

  .content {
    justify-content: center;
    flex-grow: 1;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0;
  }

  .hero {
    text-align: center;
    margin-bottom: 2rem;
  }

  h1 {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 3rem;
    font-weight: 800;
    letter-spacing: -0.04em;
    color: var(--color-base-content);
    line-height: 1.05;
    margin: 0 0 0.75rem 0;
  }

  h1 .accent {
    color: var(--color-primary);
    font-weight: 300;
  }

  .one-liner {
    font-size: 0.875rem;
    color: var(--color-neutral-content);
    text-align: center;
    line-height: 1.65;
    max-width: 28rem;
    margin: 0 auto 0 auto;
  }

  .one-liner .bold {
    color: var(--color-base-content);
    font-weight: 600;
  }

  .features {
    display: flex;
    flex-direction: column;
    gap: 0.625rem;
    width: 100%;
    margin-bottom: 2rem;
  }

  .feature-card {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 1rem 1.25rem;
    background-color: var(--color-base-200);
    border: 1px solid var(--color-base-300);
    border-radius: 0.625rem;
    width: 100%;
    transition: border-color 0.2s ease;
  }

  .feature-card:hover {
    border-color: rgba(0, 212, 184, 0.25);
  }

  .feature-icon {
    flex-shrink: 0;
    width: 2.75rem;
    height: 2.75rem;
    border-radius: 0.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .feature-text h3 {
    font-family: "Syne", system-ui, sans-serif;
    font-size: 0.9375rem;
    font-weight: 700;
    color: var(--color-base-content);
    margin: 0 0 0.2rem 0;
    letter-spacing: -0.01em;
  }

  .feature-text p {
    font-size: 0.8rem;
    color: var(--color-neutral-content);
    margin: 0;
    line-height: 1.5;
  }

  .footnote {
    font-size: 0.75rem;
    color: #4a5568;
    text-align: center;
    line-height: 1.6;
    padding: 0.75rem;
    border-top: 1px solid var(--color-base-300);
    width: 100%;
    margin-top: 0.5rem;
  }

  .footnote .bold {
    color: var(--color-neutral-content);
    font-weight: 600;
  }

  button {
    align-self: center;
    width: auto;
    font-family: "Syne", system-ui, sans-serif;
    font-weight: 700;
    text-transform: none;
    letter-spacing: 0.03em;
    background: var(--color-base-300);
    border: 1px solid #2a2a38;
    color: var(--color-neutral-content);
    margin: 1.5rem 0 1rem;
    padding: 0.875rem 3rem;
    font-size: 1rem;
    text-align: center;
    border-radius: 0.5rem;
    cursor: not-allowed;
    transition: all 0.2s ease;
  }
</style>
