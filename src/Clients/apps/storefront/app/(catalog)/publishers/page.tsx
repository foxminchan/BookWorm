import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import publishersApiClient from "@workspace/api-client/catalog/publishers";
import { catalogKeys } from "@workspace/api-hooks/keys";

import PublishersPageClient from "@/features/catalog/publisher/publishers-page-client";
import { getQueryClient } from "@/lib/query-client";

export default async function PublishersPage() {
  const queryClient = getQueryClient();

  try {
    await queryClient.prefetchQuery({
      queryKey: catalogKeys.publishers.lists(),
      queryFn: () => publishersApiClient.list(),
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
