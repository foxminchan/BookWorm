import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import type { Feedback, CreateFeedbackRequest } from "@workspace/types/rating";
import { ratingKeys } from "../keys";

export function useCreateFeedback(
  options?: UseMutationOptions<Feedback, Error, CreateFeedbackRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => ratingApiClient.createFeedback(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.byBook(data.bookId),
      });
    },
    ...options,
  });
}
