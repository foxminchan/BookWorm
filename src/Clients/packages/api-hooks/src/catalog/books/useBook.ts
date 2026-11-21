import {
  type UseQueryOptions,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";

import booksApiClient from "@workspace/api-client/catalog/books";
import { formatETag } from "@workspace/api-client/client";
import type { Book } from "@workspace/types/catalog/books";

import { fetchWithETag, getCachedETag } from "../../etag-cache";
import { catalogKeys } from "../../keys";

export default function useBook(
  id: string,
  options?: Omit<UseQueryOptions<Book>, "queryKey" | "queryFn">,
) {
  const queryClient = useQueryClient();
  const queryKey = catalogKeys.books.detail(id);

  const query = useQuery({
    queryKey,
    queryFn: () =>
      fetchWithETag(
        queryClient,
        queryKey,
        (opts) => booksApiClient.getConditional(id, opts),
        `book ${id}`,
      ),
    ...options,
  });

  const etag =
    getCachedETag(queryClient, queryKey) ?? formatETag(query.data?.version);

  return { ...query, etag };
}
