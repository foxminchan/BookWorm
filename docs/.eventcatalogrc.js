export default {
	overrides: [
		{
			files: ["*PresidioAPI*", "*SendGridAPI*"],
			rules: {
				"refs/file-exists": "off",
			},
		},
		{
			files: ["*ListBooks*"],
			rules: {
				"schema/required-fields": "off",
			},
		},
	],
};
