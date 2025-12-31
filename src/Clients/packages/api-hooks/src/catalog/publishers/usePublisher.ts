import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import type { Publisher } from "@workspace/types/catalog/publishers";
import { catalogKeys } from "../../keys";

export default function usePublisher(
  id: string,
  options?: Omit<UseQueryOptions<Publisher>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.publishers.detail(id),
    queryFn: () => publishersApiClient.get(id),
    ...options,
  });
}
