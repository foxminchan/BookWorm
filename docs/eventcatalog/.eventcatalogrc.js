module.exports = {
  rules: {
    // Schema validation rules
    "schema/required-fields": "error",
    "schema/valid-semver": "error",
    "schema/valid-email": "warn",

    // Reference validation rules
    "refs/owner-exists": "error",
    "refs/valid-version-range": "error",

    // Best practice rules
    "best-practices/summary-required": "warn",
    "best-practices/owner-required": "error",
  },

  // Ignore certain paths
  ignorePatterns: ["**/archived/**", "**/drafts/**"],

  // Override rules for specific file patterns
  overrides: [
    {
      files: ["**/experimental/**"],
      rules: {
        "best-practices/owner-required": "off",
      },
    },
  ],
};
