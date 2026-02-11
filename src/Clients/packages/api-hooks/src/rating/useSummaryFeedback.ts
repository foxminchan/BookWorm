import { type UseQueryOptions, useQuery } from "@tanstack/react-query";

import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import type { FeedbackSummary } from "@workspace/types/rating";

import { ratingKeys } from "../keys";

export default function useSummaryFeedback(
  bookId: string,
  options?: Omit<UseQueryOptions<FeedbackSummary>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: ratingKeys.feedbacks.summary(bookId),
    queryFn: () => ratingApiClient.summary(bookId),
    ...options,
  });
}
