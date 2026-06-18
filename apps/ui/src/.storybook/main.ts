import type { StorybookConfig } from "@storybook/sveltekit";

const config: StorybookConfig = {
  stories: ["../src/**/*.stories.@(js|ts|svelte)"],
  addons: [
    "@storybook/addon-svelte-csf",
    "@storybook/addon-docs",
    "@storybook/addon-a11y",
  ],
  framework: "@storybook/sveltekit",
};
export default config;
