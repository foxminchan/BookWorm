import { useQuery, type UseQueryOptions } from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import type { Feedback } from "@workspace/types/rating";
import { ratingKeys } from "../keys";

export function useFeedback(
  id: string,
  options?: Omit<UseQueryOptions<Feedback>, "queryKey" | "queryFn">,
) {
  return useQuery({
    queryKey: ratingKeys.feedbacks.detail(id),
    queryFn: () => ratingApiClient.getFeedback(id),
    ...options,
  });
}
