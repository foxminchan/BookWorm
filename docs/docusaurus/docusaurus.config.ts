import { themes as prismThemes } from "prism-react-renderer";
import type { Config } from "@docusaurus/types";
import type * as Preset from "@docusaurus/preset-classic";

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: "BookWorm",
  tagline: "Comprehensive architecture documentation for BookWorm",
  favicon: "img/favicon.ico",

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: "https://foxminchan.github.io",
  // Set the /<baseUrl>/ pathname under which your site is served
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: "/BookWorm",

  // GitHub pages deployment config.
  // If you aren't using GitHub pages, you don't need these.
  organizationName: "foxminchan", // Usually your GitHub org/user name.
  projectName: "BookWorm", // Usually your repo name.

  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",

  // Even if you don't use internationalization, you can use this field to set
  // useful metadata like html lang. For example, if your site is Chinese, you
  // may want to replace "en" with "zh-Hans".
  i18n: {
    defaultLocale: "en",
    locales: ["en"],
  },

  presets: [
    [
      "classic",
      {
        docs: {
          sidebarPath: "./sidebars.ts",
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            "https://github.com/foxminchan/BookWorm/tree/main/docs/docusaurus",
          sidebarCollapsed: false,
          showLastUpdateAuthor: true,
          showLastUpdateTime: true,
        },
        blog: {
          showReadingTime: true,
          feedOptions: {
            type: ["rss", "atom"],
            xslt: true,
          },
          // Please change this to your repo.
          // Remove this to remove the "edit this page" links.
          editUrl:
            "https://github.com/foxminchan/BookWorm/tree/main/docs/docusaurus",
          // Useful options to enforce blogging best practices
          onInlineTags: "warn",
          onInlineAuthors: "warn",
          onUntruncatedBlogPosts: "warn",
        },
        theme: {
          customCss: "./src/css/custom.css",
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    image: "img/banner.jpg",
    metadata: [
      {
        name: "description",
        content:
          "Comprehensive architecture documentation for BookWorm - a modern e-commerce platform built with .NET and microservices architecture with Agentic AI integration",
      },
      {
        name: "keywords",
        content:
          "BookWorm, e-commerce, .NET, microservices, architecture, documentation, C#, distributed systems, cloud-native, RESTful APIs, event-driven architecture, CI/CD, DevOps, software design, system architecture, Agentic AI, AI integration",
      },
      {
        name: "author",
        content: "foxminchan",
      },
      {
        property: "og:title",
        content: "BookWorm - E-commerce Architecture Documentation",
      },
      {
        property: "og:description",
        content:
          "Explore the comprehensive architecture documentation for BookWorm, a modern e-commerce platform built with .NET, microservices, and Agentic AI integration",
      },
      {
        property: "og:type",
        content: "website",
      },
      {
        property: "og:url",
        content: "https://foxminchan.github.io/BookWorm",
      },
    ],
    headTags: [
      {
        tagName: "script",
        attributes: {
          type: "application/ld+json",
        },
        innerHTML: JSON.stringify({
          "@context": "https://schema.org/",
          "@type": "Organization",
          name: "BookWorm",
          url: "https://github.com/foxminchan/BookWorm",
          logo: "https://raw.githubusercontent.com/foxminchan/BookWorm/refs/heads/main/docs/docusaurus/static/img/logo.svg",
        }),
      },
    ],
    docs: {
      sidebar: {},
    },
    zoom: {
      selector: ".markdown :not(em) > img",
      config: {
        background: {
          light: "rgb(255, 255, 255)",
          dark: "rgb(50, 50, 50)",
        },
      },
    },
    navbar: {
      hideOnScroll: true,
      title: "BookWorm",
      logo: {
        alt: "BookWorm Logo",
        src: "img/logo.svg",
        width: 32,
        height: 32,
      },
      items: [
        {
          type: "docSidebar",
          sidebarId: "architectureSidebar",
          position: "left",
          label: "Architecture",
        },
        { to: "/blog", label: "Blog", position: "left" },
        {
          href: "https://bookwormdev.netlify.app/",
          label: "Event Catalog",
          position: "left",
        },
        {
          href: "https://github.com/foxminchan/BookWorm",
          position: "right",
          className: "header-github-link",
          "aria-label": "GitHub repository",
        },
      ],
    },
    announcementBar: {
      id: "announcement-bar",
      content:
        '<a target="_blank" rel="nofollow noopener noreferrer" href="https://github.com/foxminchan/BookWorm/stargazers">⭐ Star Application on GitHub</a>',
      isCloseable: true,
    },
    footer: {
      style: "dark",
      links: [
        {
          title: "Docs",
          items: [
            {
              label: "Architecture",
              to: "/docs/intro",
            },
          ],
        },
        {
          title: "Monitoring",
          items: [
            {
              label: "SonarQue",
              href: "https://sonarcloud.io/summary/overall?id=foxminchan_BookWorm&branch=main",
            },
            {
              label: "Security",
              href: "https://github.com/foxminchan/BookWorm/security",
            },
          ],
        },
        {
          title: "More",
          items: [
            {
              label: "Blog",
              to: "/blog",
            },
            {
              label: "EventCatalog",
              href: "https://bookwormdev.netlify.app/",
            },
            {
              label: "GitHub",
              href: "https://github.com/foxminchan/BookWorm",
            },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} BookWorm Project. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ["csharp"],
    },
  } satisfies Preset.ThemeConfig,
  plugins: [
    [
      "@docusaurus/plugin-ideal-image",
      {
        quality: 70,
        max: 1030,
        min: 640,
        steps: 2,
        disableInDev: false,
      },
    ],
  ],
  markdown: {
    mermaid: true,
  },
  themes: [
    "@docusaurus/theme-mermaid",
    [
      require.resolve("@easyops-cn/docusaurus-search-local"),
      /** @type {import("@easyops-cn/docusaurus-search-local").PluginOptions} */
      {
        hashed: true,
      },
    ],
  ],
};

export default config;
