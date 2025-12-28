import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { authorsApiClient } from "@workspace/api-client";
import type { Author } from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export function useAuthor(
  id: string,
  options?: Omit<UseQueryOptions<Author>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.authors.detail(id),
    queryFn: () => authorsApiClient.getAuthor(id),
    ...options,
  });
}
