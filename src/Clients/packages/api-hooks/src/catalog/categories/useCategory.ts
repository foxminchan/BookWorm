import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import type { Category } from "@workspace/types/catalog/categories";
import { catalogKeys } from "../../keys";

export default function useCategory(
  id: string,
  options?: Omit<UseQueryOptions<Category>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.categories.detail(id),
    queryFn: () => categoriesApiClient.get(id),
    ...options,
  });
}
