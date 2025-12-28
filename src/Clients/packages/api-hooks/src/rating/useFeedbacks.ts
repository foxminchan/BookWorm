import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import type { Feedback, ListFeedbacksQuery } from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";
import { ratingKeys } from "../keys";

export function useFeedbacks(
  query?: ListFeedbacksQuery,
  options?: Omit<
    UseQueryOptions<PagedResult<Feedback>>,
    "queryKey" | "queryFn"
  >,
) {
  return useQuery({
    queryKey: ratingKeys.feedbacks.list(query),
    queryFn: () => ratingApiClient.listFeedbacks(query),
    ...options,
  });
}
