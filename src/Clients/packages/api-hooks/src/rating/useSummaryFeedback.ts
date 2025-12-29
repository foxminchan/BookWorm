import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import type { FeedbackSummary } from "@workspace/types/rating";
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import { ratingKeys } from "@/keys";

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
