import { defineUserConfig } from "vuepress";

import theme from "./theme.js";

export default defineUserConfig({
  title: "BookWorm Architecture",
  description:
    "Comprehensive architecture documentation for BookWorm using arc42",
  base: "/BookWorm/",
  lang: "en-US",
  head: [
    [
      "meta",
      {
        name: "viewport",
        content: "width=device-width,initial-scale=1,user-scalable=no",
      },
    ],
    [
      "link",
      { rel: "icon", href: "/BookWorm/favicon.ico", type: "image/x-icon" },
    ],
    ["link", { rel: "alternate icon", href: "/BookWorm/favicon-16x16.png", type: "image/png" }],
    [
      "link",
      { rel: "mask-icon", href: "/BookWorm/favicon.svg", color: "#45AAB8" },
    ],
  ],

  shouldPrefetch: false,

  theme,
});
