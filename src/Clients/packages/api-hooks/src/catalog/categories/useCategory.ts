import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { categoriesApiClient } from "@workspace/api-client";
import type { Category } from "@workspace/types/catalog/categories";
import { catalogKeys } from "../../keys";

export function useCategory(
  id: string,
  options?: Omit<UseQueryOptions<Category>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.categories.detail(id),
    queryFn: () => categoriesApiClient.getCategory(id),
    ...options,
  });
}
