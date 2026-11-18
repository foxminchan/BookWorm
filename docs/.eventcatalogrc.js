export default {
	overrides: [
		{
			files: ["*PresidioAPI*", "*SendGridAPI*"],
			rules: {
				"refs/file-exists": "off",
			},
		},
	],
};
