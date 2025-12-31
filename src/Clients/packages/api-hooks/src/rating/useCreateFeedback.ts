import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import type { Feedback, CreateFeedbackRequest } from "@workspace/types/rating";
import { ratingKeys } from "../keys";

export default function useCreateFeedback(
  options?: UseMutationOptions<Feedback, Error, CreateFeedbackRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => ratingApiClient.create(request),
    onSuccess: (data) => {
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.byBook(data.bookId),
      });
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.summary(data.bookId),
      });
    },
    ...options,
  });
}
