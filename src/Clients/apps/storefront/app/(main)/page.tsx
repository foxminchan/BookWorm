import type { Metadata } from "next";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import { catalogKeys } from "@workspace/api-hooks/keys";

import { env } from "@/env.mjs";
import BookshopPageContent from "@/features/home/bookshop-page-content";
import { getQueryClient } from "@/lib/query-client";

export const metadata: Metadata = {
  title: "BookWorm - Curated Books & Design Inspiration | Online Bookstore",
  description:
    "Discover a carefully curated collection of literature, design books, and inspiration for the modern reader. Shop fiction, non-fiction, design, science, and more.",
  keywords: [
    "books",
    "online bookstore",
    "literature",
    "design books",
    "fiction",
    "non-fiction",
    "book shop",
    "curated books",
    "best books",
  ],
  openGraph: {
    type: "website",
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books for the modern reader.",
    url: env.NEXT_PUBLIC_APP_URL || "https://bookworm.com",
    siteName: "BookWorm",
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books.",
  },
  alternates: {
    canonical: env.NEXT_PUBLIC_APP_URL || "https://bookworm.com",
  },
  robots: {
    index: true,
    follow: true,
    googleBot: {
      index: true,
      follow: true,
      "max-video-preview": -1,
      "max-image-preview": "large",
      "max-snippet": -1,
    },
  },
};

export default async function BookshopPage() {
  const queryClient = getQueryClient();

  // Prefetch featured books and categories in parallel on the server
  await Promise.all([
    queryClient.prefetchQuery({
      queryKey: catalogKeys.books.list({ pageSize: 4 }),
      queryFn: () => booksApiClient.list({ pageSize: 4 }),
    }),
    queryClient.prefetchQuery({
      queryKey: catalogKeys.categories.lists(),
      queryFn: () => categoriesApiClient.list(),
    }),
  ]);

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <BookshopPageContent />
    </HydrationBoundary>
  );
}
