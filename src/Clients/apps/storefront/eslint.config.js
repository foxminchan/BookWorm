import jsxA11y from "eslint-plugin-jsx-a11y";

import { nextJsConfig } from "@workspace/eslint-config/next-js";

/** @type {import("eslint").Linter.Config} */
export default [
  ...nextJsConfig,
  jsxA11y.flatConfigs.recommended,
  {
    languageOptions: {
      globals: {
        process: "readonly",
      },
    },
    rules: {
      "@typescript-eslint/no-unused-vars": [
        "warn",
        {
          argsIgnorePattern: "^_",
          varsIgnorePattern: "^_",
          caughtErrorsIgnorePattern: "^_",
        },
      ],
      "@typescript-eslint/no-explicit-any": "off",
      "react/no-unescaped-entities": "off",
      "react-hooks/set-state-in-effect": "off",
    },
  },
  {
    files: ["**/opengraph-image.tsx", "**/twitter-image.tsx"],
    rules: {
      "@next/next/no-img-element": "off",
    },
  },
  {
    files: [
      "**/__tests__/**",
      "**/e2e/**",
      "**/*.test.{ts,tsx}",
      "**/*.spec.{ts,tsx}",
    ],
    rules: {
      "@typescript-eslint/no-unused-vars": "off",
    },
  },
  {
    ignores: [
      ".next/**",
      ".turbo/**",
      ".features-gen/**",
      "node_modules/**",
      "coverage/**",
      "playwright-report/**",
      "test-results/**",
      "e2e/reports/**",
      "public/mockServiceWorker.js",
    ],
  },
  {
    files: ["e2e/**/*.ts", "e2e/**/*.js"],
    rules: {
      "react-hooks/rules-of-hooks": "off",
      "no-empty-pattern": "off",
    },
  },
];
