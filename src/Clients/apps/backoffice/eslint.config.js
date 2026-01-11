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
    ignores: [".next/**", "public/mockServiceWorker.js"],
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
];
