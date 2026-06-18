import type { Preview } from "@storybook/sveltekit";
import "./storybook.css";

const preview: Preview = {
  parameters: {
    backgrounds: {
      default: "dark",
      values: [
        { name: "dark", value: "#09090e" },
        { name: "base-200", value: "#111118" },
        { name: "base-300", value: "#1c1c26" },
      ],
    },
    controls: {
      matchers: {
        color: /(background|color)$/i,
        date: /Date$/i,
      },
    },
    a11y: {
      test: "todo",
    },
  },
};

export default preview;
