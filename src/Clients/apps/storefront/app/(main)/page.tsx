import { Suspense } from "react";

import type { Metadata } from "next";
import { connection } from "next/server";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import {
  bookQueries,
  categoryQueries,
} from "@workspace/api-hooks/catalog/queries";
import { createMetadata } from "@workspace/seo/metadata";

import { APP_CONFIG } from "@/config/app-config";
import { env } from "@/env.mjs";
import BookshopPageContent from "@/features/home/bookshop-page-content";
import HeroSection from "@/features/home/hero-section";
import { getCachedBooks, getCachedCategories } from "@/lib/cached-catalog";
import { getQueryClient } from "@/lib/query-client";

const appUrl = env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultUrl;

export const metadata: Metadata = createMetadata({
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
    url: appUrl,
    siteName: APP_CONFIG.name,
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "BookWorm - Curated Books & Design Inspiration",
    description:
      "Discover a carefully curated collection of literature and design books.",
  },
  alternates: {
    canonical: appUrl,
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
});

async function BookshopContent() {
  await connection();
  const queryClient = getQueryClient();

  // Prefetch featured books and categories in parallel on the server
  await Promise.all([
    queryClient
      .query({
        queryKey: bookQueries.list({ pageSize: 4 }).queryKey,
        queryFn: () => getCachedBooks({ pageSize: 4 }),
      })
      .catch((error: unknown) => {
        console.error("Failed to prefetch featured books:", error);
      }),
    queryClient
      .query({
        queryKey: categoryQueries.list().queryKey,
        queryFn: () => getCachedCategories(),
      })
      .catch((error: unknown) => {
        console.error("Failed to prefetch categories:", error);
      }),
  ]);

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <BookshopPageContent heroSlot={<HeroSection />} />
    </HydrationBoundary>
  );
}

export default function BookshopPage() {
  return (
    <Suspense fallback={<BookshopPageContent heroSlot={<HeroSection />} />}>
      <BookshopContent />
    </Suspense>
  );
}
