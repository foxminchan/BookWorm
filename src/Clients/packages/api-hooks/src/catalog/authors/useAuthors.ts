import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import authorsApiClient from "@workspace/api-client/catalog/authors";
import type { Author } from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export default function useAuthors(
  options?: Omit<UseQueryOptions<Author[]>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: catalogKeys.authors.list(),
    queryFn: () => authorsApiClient.list(),
    ...options,
  });
}
