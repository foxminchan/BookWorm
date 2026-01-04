import type { MetadataRoute } from "next";

import booksApiClient from "@workspace/api-client/catalog/books";
import categoriesApiClient from "@workspace/api-client/catalog/categories";

import { env } from "@/env.mjs";

/**
 * Enhanced dynamic sitemap that includes book and category pages.
 * Helps search engines discover all important pages.
 */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const baseUrl = env.NEXT_PUBLIC_APP_URL || "https://bookworm.com";

  // Static pages with priorities
  const staticPages: MetadataRoute.Sitemap = [
    {
      url: baseUrl,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 1,
    },
    {
      url: `${baseUrl}/shop`,
      lastModified: new Date(),
      changeFrequency: "daily",
      priority: 0.9,
    },
    {
      url: `${baseUrl}/categories`,
      lastModified: new Date(),
      changeFrequency: "weekly",
      priority: 0.8,
    },
    {
      url: `${baseUrl}/publishers`,
      lastModified: new Date(),
      changeFrequency: "weekly",
      priority: 0.8,
    },
    // Content pages
    {
      url: `${baseUrl}/about`,
      lastModified: new Date(),
      changeFrequency: "monthly",
      priority: 0.7,
    },
    {
      url: `${baseUrl}/shipping`,
      lastModified: new Date(),
      changeFrequency: "monthly",
      priority: 0.6,
    },
    {
      url: `${baseUrl}/returns`,
      lastModified: new Date(),
      changeFrequency: "monthly",
      priority: 0.6,
    },
  ];

  // Fetch books and categories dynamically (gracefully handle build-time failures)
  const [booksResult, categoriesResult] = await Promise.allSettled([
    booksApiClient.list({ pageSize: 100 }),
    categoriesApiClient.list(),
  ]);

  // Add book pages to sitemap
  const bookPages: MetadataRoute.Sitemap =
    booksResult.status === "fulfilled"
      ? booksResult.value.items?.map((book) => ({
          url: `${baseUrl}/shop/${book.id}`,
          lastModified: new Date(),
          changeFrequency: "weekly" as const,
          priority: 0.7,
        })) || []
      : [];

  // Add category pages to sitemap
  const categoryPages: MetadataRoute.Sitemap =
    categoriesResult.status === "fulfilled"
      ? categoriesResult.value?.map((category) => ({
          url: `${baseUrl}/shop?category=${encodeURIComponent(category.id)}`,
          lastModified: new Date(),
          changeFrequency: "weekly" as const,
          priority: 0.75,
        })) || []
      : [];

  return [...staticPages, ...bookPages, ...categoryPages];
}
