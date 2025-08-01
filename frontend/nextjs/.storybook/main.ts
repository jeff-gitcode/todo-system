import type { StorybookConfig } from "@storybook/nextjs-vite";

const config: StorybookConfig = {
  "stories": [
    "../stories/**/*.mdx",
    "../stories/**/*.stories.@(js|jsx|mjs|ts|tsx)",
    "../app/**/*.stories.@(js|jsx|mjs|ts|tsx)",
  ],
  "addons": [
    "@chromatic-com/storybook",
    "@storybook/addon-docs",
    "@storybook/addon-onboarding",
    "@storybook/addon-a11y",
    "@storybook/addon-vitest",
    // {
    //   name: '@storybook/addon-coverage',
    //   options: {
    //     istanbul: {
    //       exclude: ['**/components/**/index.ts'],
    //     },
    //   },
    // },
    // {
    //   name: 'storybook-addon-module-mock',
    //   options: {
    //     exclude: ['**/node_modules/@mui/**'],
    //   },
    // },
  ],
  "framework": {
    "name": "@storybook/nextjs-vite",
    "options": {}
  },
  "staticDirs": [
    "..\\public"
  ]
};
export default config;