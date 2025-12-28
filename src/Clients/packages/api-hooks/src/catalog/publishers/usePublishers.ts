import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { publishersApiClient } from "@workspace/api-client";
import type { Publisher } from "@workspace/types/catalog/publishers";
import type { PagedResult } from "@workspace/types/shared";
import { catalogKeys } from "../../keys";

export function usePublishers(
  params?: { pageIndex?: number; pageSize?: number },
  options?: Omit<
    UseQueryOptions<PagedResult<Publisher>>,
    "queryKey" | "queryFn"
  >,
) {
  return useQuery({
    queryKey: catalogKeys.publishers.list(params),
    queryFn: () => publishersApiClient.listPublishers(params),
    ...options,
  });
}
