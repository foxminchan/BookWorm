import { hopeTheme } from "vuepress-theme-hope";

import navbar from "./navbar.js";
import sidebar from "./sidebar.js";

export default hopeTheme({
  hostname: "https://foxminchan.github.io/BookWorm",
  author: {
    name: "Nhan Nguyen",
    url: "https://github.com/foxminchan",
  },
  logo: "/assets/image/book.svg",
  repo: "foxminchan/BookWorm",
  docsDir: "docs/src",
  navbar,
  sidebar,
  footer:
    "Licensed under the <a href='https://opensource.org/licenses/MIT' target='_blank'>MIT License</a> | © 2025 BookWorm Contributors",
  displayFooter: true,
  metaLocales: {
    editLink: "Edit this page on GitHub",
  },
  markdown: {
    align: true,
    attrs: true,
    codeTabs: true,
    component: true,
    demo: true,
    figure: true,
    gfm: true,
    imgLazyload: true,
    imgSize: true,
    include: true,
    mark: true,
    markmap: true,
    flowchart: true,
    plantuml: true,
    spoiler: true,
    stylize: [
      {
        matcher: "Recommended",
        replacer: ({ tag }) => {
          if (tag === "em")
            return {
              tag: "Badge",
              attrs: { type: "tip" },
              content: "Recommended",
            };
        },
      },
    ],
    sub: true,
    sup: true,
    tabs: true,
    tasklist: true,
    vPre: true,
    mermaid: true,
  },

  plugins: {
    catalog: true,
    components: {
      components: ["Badge", "VPCard"],
    },
    copyright: {
      author: "Nhan Nguyen",
      license: "MIT",
    },
    icon: {
      prefix: "fa6-solid:",
    },
  },
});
