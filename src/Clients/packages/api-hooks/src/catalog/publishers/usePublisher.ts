import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { publishersApiClient } from "@workspace/api-client";
import type { Publisher } from "@workspace/types/catalog/publishers";
import { catalogKeys } from "../../keys";

export function usePublisher(
  id: string,
  options?: Omit<UseQueryOptions<Publisher>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.publishers.detail(id),
    queryFn: () => publishersApiClient.getPublisher(id),
    ...options,
  });
}
