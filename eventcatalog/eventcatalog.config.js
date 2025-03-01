import path from "path";
import url from "url";

const __dirname = path.dirname(url.fileURLToPath(import.meta.url));

/** @type {import('@eventcatalog/core/bin/eventcatalog.config').Config} */
export default {
  cId: "888dcc93-8c83-4052-beee-00a7ed6d71ae",
  title: "BookWorm EventCatalog",
  tagline:
    "This internal platform provides a comprehensive view of our event-driven architecture across all systems. Use this portal to discover existing domains, explore services and their dependencies, and understand the message contracts that connect our infrastructure",
  organizationName: "BookWorm",
  homepageLink: "https://github.com/foxminchan/BookWorm/",
  editUrl: "https://github.com/foxminchan/BookWorm/edit/main/",
  trailingSlash: false,
  base: "/",
  mermaid: {
    iconPacks: ["logos"],
  },
  logo: {
    alt: "EventCatalog Logo",
    src: "/logo.png",
    text: "EventCatalog",
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
  chat: {
    enabled: true,
    model: "Hermes-3-Llama-3.2-3B-q4f16_1-MLC",
    max_tokens: 4096,
    similarityResults: 50,
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
  ],
};
