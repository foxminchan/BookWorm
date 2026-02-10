import type { Metadata } from "next";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import booksApiClient from "@workspace/api-client/catalog/books";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import { catalogKeys } from "@workspace/api-hooks/keys";

import { env } from "@/env.mjs";
import ShopPageClient from "@/features/catalog/shop/shop-page-client";
import { PRICE_RANGE } from "@/lib/constants";
import { getShopSortParams } from "@/lib/pattern";
import { getQueryClient } from "@/lib/query-client";
import { buildQueryString } from "@/lib/seo";

const ITEMS_PER_PAGE = 8;

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
  const url = `${env.NEXT_PUBLIC_APP_URL || "https://bookworm.com"}/shop`;

  let title = "Shop Books | BookWorm";
  let description =
    "Browse our curated collection of books across various genres and categories.";
  const keywords = ["books", "online bookstore", "buy books", "literature"];

  // Customize metadata based on filters
  if (params.search) {
    title = `Search: "${params.search}" | BookWorm`;
    description = `Search results for "${params.search}" in our book collection.`;
    keywords.push(params.search);
  } else if (params.category) {
    const categories = await categoriesApiClient.list();
    const category = categories.find((c) => c.id === params.category);
    if (category) {
      title = `${category.name} Books | BookWorm`;
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
      title,
      description,
      url: canonicalUrl,
      siteName: "BookWorm",
      locale: "en_US",
    },
    twitter: {
      card: "summary",
      title,
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

export default async function ShopPage({
  searchParams,
}: Readonly<ShopPageProps>) {
  const params = await searchParams;
  const queryClient = getQueryClient();

  const currentPage = parsePageNumber(params.page);
  const sortBy = params.sort ?? "name";
  const sortParams = getShopSortParams(sortBy);

  const bookListParams = {
    pageIndex: currentPage,
    pageSize: ITEMS_PER_PAGE,
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
      queryClient.prefetchQuery({
        queryKey: catalogKeys.books.list(bookListParams),
        queryFn: () => booksApiClient.list(bookListParams),
      }),
      queryClient.prefetchQuery({
        queryKey: catalogKeys.categories.lists(),
        queryFn: () => categoriesApiClient.list(),
      }),
      queryClient.prefetchQuery({
        queryKey: catalogKeys.publishers.lists(),
        queryFn: () => publishersApiClient.list(),
      }),
      queryClient.prefetchQuery({
        queryKey: catalogKeys.authors.lists(),
        queryFn: () => authorsApiClient.list(),
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
