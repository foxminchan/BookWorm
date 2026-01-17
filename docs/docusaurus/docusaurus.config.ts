import type * as Preset from "@docusaurus/preset-classic";
import type { Config } from "@docusaurus/types";
import { themes as prismThemes } from "prism-react-renderer";

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
          editUrl: "https://github.com/foxminchan/BookWorm/tree/main/docs/docusaurus",
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
          editUrl: "https://github.com/foxminchan/BookWorm/tree/main/docs/docusaurus",
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
          "Comprehensive architecture documentation for BookWorm - a modern e-commerce platform built with .NET 10, microservices architecture, Azure OpenAI, and Microsoft Agents AI Framework. Features Next.js 16 frontend, TUnit testing, and Hybrid Caching.",
      },
      {
        name: "keywords",
        content:
          "BookWorm, e-commerce, .NET 10, microservices, architecture, documentation, C#, distributed systems, cloud-native, Azure OpenAI, GPT-4o-mini, Semantic Kernel, Microsoft Agents AI Framework, MCP, A2A Protocol, Next.js 16, React 19, Turbo, TUnit, Hybrid Caching, Aspire, PostgreSQL, RabbitMQ, Keycloak, CQRS, event-driven architecture, arc42, ADR, technical documentation",
      },
      {
        name: "author",
        content: "Nhan Nguyen (foxminchan)",
      },
      {
        name: "robots",
        content: "index, follow, max-image-preview:large, max-snippet:-1, max-video-preview:-1",
      },
      {
        name: "language",
        content: "English",
      },
      {
        name: "revisit-after",
        content: "7 days",
      },
      {
        property: "og:title",
        content: "BookWorm - Modern .NET 10 E-commerce Architecture Documentation",
      },
      {
        property: "og:description",
        content:
          "Explore comprehensive architecture documentation for BookWorm: .NET 10 microservices with Azure OpenAI integration, Next.js 16 + React 19 frontend, TUnit testing framework, and Hybrid Caching. Complete arc42 documentation with ADRs and deployment guides.",
      },
      {
        property: "og:type",
        content: "website",
      },
      {
        property: "og:url",
        content: "https://foxminchan.github.io/BookWorm",
      },
      {
        property: "og:image",
        content: "https://foxminchan.github.io/BookWorm/img/banner.jpg",
      },
      {
        property: "og:image:width",
        content: "1200",
      },
      {
        property: "og:image:height",
        content: "630",
      },
      {
        property: "og:site_name",
        content: "BookWorm Documentation",
      },
      {
        property: "og:locale",
        content: "en_US",
      },
      {
        name: "twitter:card",
        content: "summary_large_image",
      },
      {
        name: "twitter:title",
        content: "BookWorm - Modern .NET 10 E-commerce Architecture",
      },
      {
        name: "twitter:description",
        content:
          "Comprehensive architecture documentation: .NET 10 microservices, Azure OpenAI, Next.js 16, TUnit, Hybrid Caching. arc42 + ADRs.",
      },
      {
        name: "twitter:image",
        content: "https://foxminchan.github.io/BookWorm/img/banner.jpg",
      },
      {
        name: "twitter:creator",
        content: "@foxminchan",
      },
    ],
    headTags: [
      {
        tagName: "link",
        attributes: {
          rel: "canonical",
          href: "https://foxminchan.github.io/BookWorm",
        },
      },
      {
        tagName: "meta",
        attributes: {
          name: "theme-color",
          content: "#25c2a0",
        },
      },
      {
        tagName: "script",
        attributes: {
          type: "application/ld+json",
        },
        innerHTML: JSON.stringify({
          "@context": "https://schema.org",
          "@type": "TechArticle",
          headline: "BookWorm E-commerce Architecture Documentation",
          description:
            "Comprehensive architecture documentation for a modern e-commerce platform built with .NET 10 microservices, Azure OpenAI, Next.js 16, and Microsoft Agents AI Framework",
          author: {
            "@type": "Person",
            name: "Nhan Nguyen",
            url: "https://github.com/foxminchan",
          },
          publisher: {
            "@type": "Organization",
            name: "BookWorm Project",
            url: "https://github.com/foxminchan/BookWorm",
            logo: {
              "@type": "ImageObject",
              url: "https://foxminchan.github.io/BookWorm/img/logo.svg",
            },
          },
          datePublished: "2024-07-01",
          dateModified: new Date().toISOString().split("T")[0],
          inLanguage: "en-US",
          keywords:
            ".NET 10, microservices, Azure OpenAI, architecture documentation, arc42, Next.js 16, React 19, TUnit, Hybrid Caching, CQRS, event-driven architecture",
          about: {
            "@type": "SoftwareApplication",
            name: "BookWorm",
            applicationCategory: "E-commerce Platform",
            operatingSystem: "Cross-platform",
            softwareVersion: "1.0",
            programmingLanguage: ["C#", "TypeScript"],
            offers: {
              "@type": "Offer",
              price: "0",
              priceCurrency: "USD",
            },
          },
          mainEntity: {
            "@type": "WebSite",
            name: "BookWorm Documentation",
            url: "https://foxminchan.github.io/BookWorm",
            potentialAction: {
              "@type": "SearchAction",
              target: "https://foxminchan.github.io/BookWorm/search?q={search_term_string}",
              "query-input": "required name=search_term_string",
            },
          },
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
    [
      "@docusaurus/plugin-sitemap",
      {
        changefreq: "weekly",
        priority: 0.5,
        ignorePatterns: ["/tags/**"],
        filename: "sitemap.xml",
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
