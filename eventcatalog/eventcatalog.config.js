import path from "path";
import url from "url";

const __dirname = path.dirname(url.fileURLToPath(import.meta.url));

/** @type {import('@eventcatalog/core/bin/eventcatalog.config').Config} */
export default {
  cId: "888dcc93-8c83-4052-beee-00a7ed6d71ae",
  title: "BookWorm",
  tagline:
    "This internal platform provides a comprehensive view of our event-driven architecture across all systems. Use this portal to discover existing domains, explore services and their dependencies, and understand the message contracts that connect our infrastructure",
  organizationName: "BookWorm",
  homepageLink: "https://github.com/foxminchan/BookWorm/",
  editUrl: "https://github.com/foxminchan/BookWorm/edit/main/",
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
  docs: {
    sidebar: {
      type: "LIST_VIEW",
    },
  },
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
      "@eventcatalog/generator-ai",
      {
        splitMarkdownFiles: true,
        includeUsersAndTeams: true,
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "basket-api.yml"),
            id: "Basket Service",
          },
        ],
        domain: { id: "basket", name: "Basket", version: "1.0.0" },
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
        ],
        domain: { id: "basket", name: "Basket", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "catalog-api.yml"),
            id: "Product Service",
          },
        ],
        domain: { id: "catalog", name: "Catalog", version: "1.0.0" },
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
        ],
        domain: { id: "catalog", name: "Catalog", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "finance-api.yml"),
            id: "Finance Service",
          },
        ],
        domain: { id: "finance", name: "Finance", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          {
            path: path.join(__dirname, "asyncapi-files", "finance-service.yml"),
            id: "Finance Service",
          },
        ],
        domain: { id: "finance", name: "Finance", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          {
            path: path.join(
              __dirname,
              "asyncapi-files",
              "notification-service.yml"
            ),
            id: "Notification Service",
          },
        ],
        domain: { id: "notification", name: "Notification", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "ordering-api.yml"),
            id: "Ordering Service",
          },
        ],
        domain: { id: "ordering", name: "Ordering", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          {
            path: path.join(
              __dirname,
              "asyncapi-files",
              "ordering-service.yml"
            ),
            id: "Ordering Service",
          },
        ],
        domain: { id: "ordering", name: "Ordering", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "rating-api.yml"),
            id: "Rating Service",
          },
        ],
        domain: { id: "rating", name: "Rating", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-asyncapi",
      {
        services: [
          {
            path: path.join(__dirname, "asyncapi-files", "rating-service.yml"),
            id: "Rating Service",
          },
        ],
        domain: { id: "rating", name: "Rating", version: "1.0.0" },
      },
    ],
    [
      "@eventcatalog/generator-openapi",
      {
        services: [
          {
            path: path.join(__dirname, "openapi-files", "chat-api.yml"),
            id: "Chat Service",
          },
        ],
        domain: { id: "chat", name: "Chat", version: "1.0.0" },
      },
    ],
  ],
  visualiser: {
    channels: {
      renderMode: "flat",
    },
  },
};
