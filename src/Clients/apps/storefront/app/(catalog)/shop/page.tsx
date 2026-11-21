import { Suspense } from "react";

import type { Metadata } from "next";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import {
  authorQueries,
  bookQueries,
  categoryQueries,
  publisherQueries,
} from "@workspace/api-hooks/catalog/queries";
import { PRICE_RANGE } from "@workspace/utils/constants";

import { APP_CONFIG, SHOP_ITEMS_PER_PAGE } from "@/config/app-config";
import { env } from "@/env.mjs";
import ShopPageClient from "@/features/catalog/shop/shop-page-client";
import {
  getCachedAuthors,
  getCachedBooks,
  getCachedCategories,
  getCachedPublishers,
} from "@/lib/cached-catalog";
import { getShopSortParams } from "@/lib/pattern";
import { getQueryClient } from "@/lib/query-client";
import { buildQueryString } from "@/lib/seo";

import ShopLoading from "./loading";

function parsePageNumber(page: string | undefined): number {
  return Number.parseInt(page ?? "1", 10);
}

type ShopPageProps = {
  searchParams: Promise<{
    category?: string;
    publisher?: string;
    author?: string;
    search?: string;
    page?: string;
    sort?: string;
  }>;
};

/**
 * Generates dynamic metadata for shop page based on filters.
 * Optimizes SEO for filtered and search result pages.
 */
export async function generateMetadata({
  searchParams,
}: ShopPageProps): Promise<Metadata> {
  const params = await searchParams;
  const url = `${env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultUrl}/shop`;

  let title = "Shop Books";
  let description =
    "Browse our curated collection of books across various genres and categories.";
  const keywords = ["books", "online bookstore", "buy books", "literature"];

  // Customize metadata based on filters
  if (params.search) {
    title = `Search: "${params.search}"`;
    description = `Search results for "${params.search}" in our book collection.`;
    keywords.push(params.search);
  } else if (params.category) {
    const categories = await getCachedCategories();
    const category = categories.find((c) => c.id === params.category);
    if (category) {
      title = `${category.name} Books`;
      description = `Explore our collection of ${category.name} books. Find your next great read in this category.`;
      keywords.push(category.name ?? "Unknown Category");
    }
  }

  if (params.page && params.page !== "1") {
    title = `${title} - Page ${params.page}`;
  }

  // Build canonical URL with normalized query params
  const queryString = buildQueryString({
    category: params.category,
    publisher: params.publisher,
    author: params.author,
    search: params.search,
    sort: params.sort,
    page: params.page,
  });

  const canonicalUrl = queryString ? `${url}?${queryString}` : url;

  // Generate prev/next pagination links
  const currentPage = parsePageNumber(params.page);
  const searchParamsObj = new URLSearchParams(queryString);
  const prevParams = new URLSearchParams(searchParamsObj);
  const nextParams = new URLSearchParams(searchParamsObj);

  if (currentPage > 1) {
    if (currentPage === 2) {
      prevParams.delete("page");
    } else {
      prevParams.set("page", String(currentPage - 1));
    }
  }
  nextParams.set("page", String(currentPage + 1));

  const prevUrl =
    currentPage > 1 ? `${url}?${prevParams.toString()}` : undefined;
  const nextUrl = `${url}?${nextParams.toString()}`;

  return {
    title,
    description,
    keywords,
    openGraph: {
      type: "website",
      title: `${title} | ${APP_CONFIG.name}`,
      description,
      url: canonicalUrl,
      siteName: APP_CONFIG.name,
      locale: "en_US",
    },
    twitter: {
      card: "summary",
      title: `${title} | ${APP_CONFIG.name}`,
      description,
    },
    alternates: {
      canonical: canonicalUrl,
    },
    other: {
      // Pagination links for SEO
      ...(prevUrl && { prev: prevUrl }),
      next: nextUrl,
    },
    robots: {
      index: currentPage === 1, // Only index first page to avoid duplicate content
      follow: true,
      googleBot: {
        index: currentPage === 1,
        follow: true,
        "max-video-preview": -1,
        "max-image-preview": "large",
        "max-snippet": -1,
      },
    },
  };
}

async function ShopContent({ searchParams }: Readonly<ShopPageProps>) {
  const params = await searchParams;
  const queryClient = getQueryClient();

  const currentPage = parsePageNumber(params.page);
  const sortBy = params.sort ?? "name";
  const sortParams = getShopSortParams(sortBy);

  const bookListParams = {
    pageIndex: currentPage,
    pageSize: SHOP_ITEMS_PER_PAGE,
    categoryId: params.category,
    publisherId: params.publisher,
    authorId: params.author,
    search: params.search,
    minPrice: PRICE_RANGE.min,
    maxPrice: PRICE_RANGE.max,
    orderBy: sortParams.orderBy,
    isDescending: sortParams.isDescending,
  };

  // Prefetch all necessary data in parallel
  try {
    await Promise.all([
      queryClient.query({
        queryKey: bookQueries.list(bookListParams).queryKey,
        queryFn: () => getCachedBooks(bookListParams),
      }),
      queryClient.query({
        queryKey: categoryQueries.list().queryKey,
        queryFn: () => getCachedCategories(),
      }),
      queryClient.query({
        queryKey: publisherQueries.list().queryKey,
        queryFn: () => getCachedPublishers(),
      }),
      queryClient.query({
        queryKey: authorQueries.list().queryKey,
        queryFn: () => getCachedAuthors(),
      }),
    ]);
  } catch (error) {
    console.error("Failed to prefetch shop data:", error);
  }

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <ShopPageClient />
    </HydrationBoundary>
  );
}

export default function ShopPage({ searchParams }: Readonly<ShopPageProps>) {
  return (
    <Suspense fallback={<ShopLoading />}>
      <ShopContent searchParams={searchParams} />
    </Suspense>
  );
}
