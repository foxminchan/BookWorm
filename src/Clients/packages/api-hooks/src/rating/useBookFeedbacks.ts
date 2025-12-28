import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import type { Feedback } from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";
import { ratingKeys } from "../keys";

export function useBookFeedbacks(
  bookId: string,
  params?: { pageIndex?: number; pageSize?: number },
  options?: Omit<
    UseQueryOptions<PagedResult<Feedback>>,
    "queryKey" | "queryFn"
  >,
) {
  return useQuery({
    queryKey: ratingKeys.feedbacks.byBook(bookId, params),
    queryFn: () => ratingApiClient.getBookFeedbacks(bookId, params),
    ...options,
  });
}
