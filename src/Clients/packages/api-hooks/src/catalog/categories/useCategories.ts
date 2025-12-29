import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type { Category } from "@workspace/types/catalog/categories";
import { catalogKeys } from "@/keys";

export default function useCategories(
  options?: Omit<UseQueryOptions<Category[]>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.categories.list(),
    queryFn: () => categoriesApiClient.list(),
    ...options,
  });
}
