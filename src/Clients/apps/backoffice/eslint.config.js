import jsxA11y from "eslint-plugin-jsx-a11y";

import { nextJsConfig } from "@workspace/eslint-config/next-js";

/** @type {import("eslint").Linter.Config} */
export default [
  ...nextJsConfig,
  {
    plugins: {
      "jsx-a11y": jsxA11y,
    },
  },
  {
    ignores: [
      ".next/**",
      "public/mockServiceWorker.js",
      "coverage/**",
      "**/coverage/**",
      "coverage/lcov-report/**",
    ],
  },
  {
    rules: {
      "react-hooks/incompatible-library": "off",
      "react-hooks/set-state-in-effect": "off",
      "react-hooks/immutability": "off",
      "@typescript-eslint/no-unused-vars": "off",
      "no-undef": "off",
    },
  },
  {
    files: ["**/__tests__/**/*.test.tsx", "**/__tests__/**/*.test.ts"],
    rules: {
      "@typescript-eslint/no-explicit-any": "off",
    },
  },
];
