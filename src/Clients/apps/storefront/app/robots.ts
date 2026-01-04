import type { MetadataRoute } from "next";

import { env } from "@/env.mjs";

/**
 * Enhanced robots.txt configuration for better SEO and crawl control.
 * Allows major search engines while blocking AI bots and protecting sensitive routes.
 */
export default function robots(): MetadataRoute.Robots {
  const baseUrl = env.NEXT_PUBLIC_APP_URL || "https://bookworm.com";

  return {
    rules: [
      {
        userAgent: "*",
        allow: "/",
        disallow: [
          "/account/*",
          "/checkout/*",
          "/api/*",
          "/basket",
          "/login",
          "/register",
        ],
        crawlDelay: 1,
      },
      // Block AI training bots
      {
        userAgent: "GPTBot",
        disallow: "/",
      },
      {
        userAgent: "CCBot",
        disallow: "/",
      },
      {
        userAgent: "ChatGPT-User",
        disallow: "/",
      },
      {
        userAgent: "anthropic-ai",
        disallow: "/",
      },
      {
        userAgent: "Claude-Web",
        disallow: "/",
      },
      // Optimize for major search engines
      {
        userAgent: "Googlebot",
        allow: "/",
        disallow: ["/account/*", "/checkout/*", "/api/*", "/basket"],
      },
      {
        userAgent: "Bingbot",
        allow: "/",
        disallow: ["/account/*", "/checkout/*", "/api/*", "/basket"],
      },
    ],
    sitemap: `${baseUrl}/sitemap.xml`,
    host: baseUrl,
  };
}
