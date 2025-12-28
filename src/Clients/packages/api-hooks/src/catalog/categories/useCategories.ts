import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { categoriesApiClient } from "@workspace/api-client";
import type { Category } from "@workspace/types/catalog/categories";
import type { PagedResult } from "@workspace/types/shared";
import { catalogKeys } from "../../keys";

export function useCategories(
  params?: { pageIndex?: number; pageSize?: number },
  options?: Omit<
    UseQueryOptions<PagedResult<Category>>,
    "queryKey" | "queryFn"
  >,
) {
  return useQuery({
    queryKey: catalogKeys.categories.list(params),
    queryFn: () => categoriesApiClient.listCategories(params),
    ...options,
  });
}
