// @ts-check
import eslint from "@eslint/js";
import tseslint from "typescript-eslint";
import prettierConfig from "eslint-config-prettier";

export default tseslint.config(
	eslint.configs.recommended,
	...tseslint.configs.recommended,
	{
		ignores: ["node_modules/**", "dist/**", "*.config.js", "webpack.config.js"],
	},
	{
		files: ["src/**/*.ts", "src/**/*.js"],
		languageOptions: {
			parserOptions: {
				ecmaVersion: "latest",
				sourceType: "module",
				project: "./tsconfig.json",
			},
			globals: {
				__ENV: "readonly",
				__VU: "readonly",
				__ITER: "readonly",
				open: "readonly",
				console: "readonly",
			},
		},
		rules: {
			"@typescript-eslint/explicit-function-return-type": "off",
			"@typescript-eslint/no-explicit-any": "warn",
			"@typescript-eslint/no-unused-vars": [
				"error",
				{
					argsIgnorePattern: "^_",
					varsIgnorePattern: "^_",
				},
			],
			"no-console": "off",
			"prefer-const": "error",
			"no-var": "error",
		},
	},
	prettierConfig
);
