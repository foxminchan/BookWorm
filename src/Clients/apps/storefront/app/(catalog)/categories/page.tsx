import { HydrationBoundary, dehydrate } from "@tanstack/react-query";

import categoriesApiClient from "@workspace/api-client/catalog/categories";
import { catalogKeys } from "@workspace/api-hooks/keys";

import CategoriesPageClient from "@/features/catalog/categories/categories-page-client";
import { getQueryClient } from "@/lib/query-client";

export default async function CategoriesPage() {
  const queryClient = getQueryClient();

  try {
    await queryClient.prefetchQuery({
      queryKey: catalogKeys.categories.lists(),
      queryFn: () => categoriesApiClient.list(),
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
