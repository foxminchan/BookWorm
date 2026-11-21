import { Suspense } from "react";

import type { Metadata } from "next";
import { connection } from "next/server";

import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import { publisherQueries } from "@workspace/api-hooks/catalog/queries";
import { createMetadata } from "@workspace/seo/metadata";

import PublishersPageClient from "@/features/catalog/publisher/publishers-page-client";
import { getCachedPublishers } from "@/lib/cached-catalog";
import { getQueryClient } from "@/lib/query-client";

import PublishersLoading from "./loading";

export const metadata: Metadata = createMetadata({
  title: "Publishers",
  description:
    "Discover independent and renowned book publishers featured in the BookWorm catalog.",
});

async function PublishersContent() {
  await connection();
  const queryClient = getQueryClient();

  try {
    await queryClient.query({
      queryKey: publisherQueries.list().queryKey,
      queryFn: () => getCachedPublishers(),
    });
  } catch (error) {
    console.error("Failed to prefetch publishers:", error);
  }

  return (
    <HydrationBoundary state={dehydrate(queryClient)}>
      <PublishersPageClient />
    </HydrationBoundary>
  );
}

export default function PublishersPage() {
  return (
    <Suspense fallback={<PublishersLoading />}>
      <PublishersContent />
    </Suspense>
  );
}
