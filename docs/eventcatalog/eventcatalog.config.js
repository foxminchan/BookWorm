/** @type {import('@eventcatalog/core/bin/eventcatalog.config').Config} */
export default {
	cId: "888dcc93-8c83-4052-beee-00a7ed6d71ae",
	title: "BookWorm",
	tagline:
		"This internal platform provides a comprehensive view of our event-driven architecture across all systems. Use this portal to discover existing domains, explore services and their dependencies, and understand the message contracts that connect our infrastructure",
	organizationName: "BookWorm",
	homepageLink: "https://github.com/foxminchan/BookWorm/",
	repositoryUrl: "https://github.com/foxminchan/BookWorm",
	editUrl: "https://github.com/foxminchan/BookWorm/edit/main/docs/eventcatalog",
	theme: "sapphire",
	trailingSlash: false,
	base: "/",
	mdxOptimize: true,
	mermaid: {
		iconPacks: ["logos"],
	},
	logo: {
		alt: "BookWorm",
		src: "/logo.svg",
		text: "BookWorm",
	},
	rss: {
		enabled: true,
		limit: 20,
	},
	docs: {},
	sidebar: [
		{
			id: "/chat",
			visible: false,
		},
		{
			id: "/docs/custom",
			visible: false,
		},
	],
	api: {
		fullCatalogAPIEnabled: true,
	},
	visualiser: {
		channels: {
			renderMode: "flat",
		},
	},
	environments: [
		{
			name: "Production",
			url: "https://bookwormdev.netlify.app/",
			description: "Production environment",
			shortName: "Prod",
		},
	],
	llmsTxt: {
		enabled: true,
	},
};
