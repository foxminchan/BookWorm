import js from "@eslint/js";
import eslintPluginAstro from "eslint-plugin-astro";
import jsxA11y from "eslint-plugin-jsx-a11y";
import prettierConfig from "eslint-config-prettier";

export default [
	js.configs.recommended,
	...eslintPluginAstro.configs.recommended,
	jsxA11y.flatConfigs.recommended,
	prettierConfig,
	{
		files: ["**/*.{js,mjs,cjs,ts,tsx,astro}"],
		languageOptions: {
			ecmaVersion: "latest",
			sourceType: "module",
			globals: {
				// EventCatalog globals
				frontmatter: "readonly",
			},
		},
		rules: {
			"no-unused-vars": ["warn", { argsIgnorePattern: "^_" }],
			"no-console": ["warn", { allow: ["warn", "error"] }],
		},
	},
	{
		// CommonJS files like .eventcatalogrc.js
		files: [".eventcatalogrc.js"],
		languageOptions: {
			ecmaVersion: "latest",
			sourceType: "commonjs",
			globals: {
				module: "readonly",
				require: "readonly",
				__dirname: "readonly",
				__filename: "readonly",
			},
		},
	},
	{
		ignores: [
			"dist/",
			"build/",
			".astro/",
			".eventcatalog-core/",
			"node_modules/",
			"public/generated/",
		],
	},
];
