import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { authorsApiClient } from "@workspace/api-client";
import type { Author } from "@workspace/types/catalog/authors";
import type { PagedResult } from "@workspace/types/shared";
import { catalogKeys } from "../../keys";

export function useAuthors(
  params?: { pageIndex?: number; pageSize?: number },
  options?: Omit<UseQueryOptions<PagedResult<Author>>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.authors.list(params),
    queryFn: () => authorsApiClient.listAuthors(params),
    ...options,
  });
}
