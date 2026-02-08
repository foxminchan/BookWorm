import path from "path";
import url from "url";

const __dirname = path.dirname(url.fileURLToPath(import.meta.url));
const generateMarkdownForService = ({ service, markdown, document }) => {
	return `# ${service.name}

    ## Description
    ${document.info.description ? `${document.info.description}` : ""}

    ## Details
    ${markdown}
  `;
};
const generateMarkdownForMessage = ({ message, markdown, document }) => {
	return `# ${message.name}

    ## Description
    ${document.info.description ? `${document.info.description}` : ""}

    ## Details
    ${markdown}
  `;
};
const generateMarkdownForDomain = ({ domain, markdown }) => {
	return `# ${domain.name}

    ## Overview
    ${markdown}

    <Tiles>
      <Tile
        icon="UserGroupIcon"
        href="/docs/users/nhanxnguyen"
        title="Contact the author"
        description="Any questions? Feel free to contact the owners"
      />
      <Tile
        icon="RectangleGroupIcon"
        href="/visualiser/domains/${frontmatter.id}/${frontmatter.version}"
        title="${frontmatter.services.length} services are in this domain"
        description="Explore the services in this domain"
      />
    </Tiles>

    ## Architecture Diagram
    <NodeGraph />
`;
};

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
	generators: [
		[
			"@eventcatalog/generator-openapi",
			{
				services: [
					{
						path: path.join(__dirname, "openapi-files", "basket-api.yml"),
						id: "Basket Service",
						name: "Basket Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
					{
						path: path.join(__dirname, "openapi-files", "finance-api.yml"),
						id: "Finance Service",
						name: "Finance Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
					{
						path: path.join(__dirname, "openapi-files", "ordering-api.yml"),
						id: "Ordering Service",
						name: "Ordering Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
				],
				domain: {
					id: "orders",
					name: "Orders",
					version: "1.0.0",
					generateMarkdown: generateMarkdownForDomain,
				},
				sidebarBadgeType: "HTTP_METHOD",
				httpMethodsToMessages: {
					GET: "query",
					POST: "command",
					PUT: "command",
					DELETE: "command",
				},
			},
		],
		[
			"@eventcatalog/generator-openapi",
			{
				services: [
					{
						path: path.join(__dirname, "openapi-files", "catalog-api.yml"),
						id: "Product Service",
						name: "Product Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
					{
						path: path.join(__dirname, "openapi-files", "rating-api.yml"),
						id: "Rating Service",
						name: "Rating Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
					{
						path: path.join(__dirname, "openapi-files", "chat-api.yml"),
						id: "Chat Service",
						name: "Chat Service",
						version: "1.0.0",
						generateMarkdown: generateMarkdownForService,
					},
				],
				domain: {
					id: "catalog",
					name: "Catalog",
					version: "1.0.0",
					generateMarkdown: generateMarkdownForDomain,
				},
				sidebarBadgeType: "HTTP_METHOD",
				httpMethodsToMessages: {
					GET: "query",
					POST: "command",
					PUT: "command",
					DELETE: "command",
				},
			},
		],
		[
			"@eventcatalog/generator-asyncapi",
			{
				services: [
					{
						path: path.join(__dirname, "asyncapi-files", "basket-service.yml"),
						id: "Basket Service",
					},
					{
						path: path.join(__dirname, "asyncapi-files", "finance-service.yml"),
						id: "Finance Service",
					},
					{
						path: path.join(__dirname, "asyncapi-files", "notification-service.yml"),
						id: "Notification Service",
					},
					{
						path: path.join(__dirname, "asyncapi-files", "ordering-service.yml"),
						id: "Ordering Service",
					},
				],
				messages: {
					generateMarkdown: generateMarkdownForMessage,
				},
				sidebarBadgeType: "MESSAGE_TYPE",
				domain: { id: "orders", name: "Orders", version: "1.0.0" },
				debug: true,
			},
		],
		[
			"@eventcatalog/generator-asyncapi",
			{
				services: [
					{
						path: path.join(__dirname, "asyncapi-files", "catalog-service.yml"),
						id: "Product Service",
					},
					{
						path: path.join(__dirname, "asyncapi-files", "rating-service.yml"),
						id: "Rating Service",
					},
				],
				messages: {
					generateMarkdown: generateMarkdownForMessage,
				},
				domain: { id: "catalog", name: "Catalog", version: "1.0.0" },
				debug: true,
			},
		],
	],
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
