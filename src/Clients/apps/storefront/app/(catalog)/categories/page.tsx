import { Suspense } from "react";

import type { Metadata } from "next";
import { connection } from "next/server";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import { categoryQueries } from "@workspace/api-hooks/catalog/queries";
import { createMetadata } from "@workspace/seo/metadata";

import CategoriesPageClient from "@/features/catalog/categories/categories-page-client";
import { getCachedCategories } from "@/lib/cached-catalog";
import { getQueryClient } from "@/lib/query-client";

import CategoriesLoading from "./loading";

export const metadata: Metadata = createMetadata({
  title: "Categories",
  description:
    "Explore book categories across fiction, non-fiction, design, science, and more at BookWorm.",
});

async function CategoriesContent() {
  await connection();
  const queryClient = getQueryClient();

  try {
    await queryClient.query({
      queryKey: categoryQueries.list().queryKey,
      queryFn: () => getCachedCategories(),
    });
  } catch (error) {
    console.error("Failed to prefetch categories:", error);
  }

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <CategoriesPageClient />
    </HydrationBoundary>
  );
}

export default function CategoriesPage() {
  return (
    <Suspense fallback={<CategoriesLoading />}>
      <CategoriesContent />
    </Suspense>
  );
}
