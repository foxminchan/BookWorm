import { config as baseConfig } from "@workspace/eslint-config/base";

/** @type {import("eslint").Linter.Config} */
export default [
  ...baseConfig,
  {
    ignores: ["apps/**", "packages/**"],
  },
];
