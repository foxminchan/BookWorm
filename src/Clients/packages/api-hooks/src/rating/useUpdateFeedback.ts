import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import type { Feedback, CreateFeedbackRequest } from "@workspace/types/rating";
import { ratingKeys } from "../keys";

export function useUpdateFeedback(
  options?: UseMutationOptions<
    Feedback,
    Error,
    { id: string; request: CreateFeedbackRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) =>
      ratingApiClient.updateFeedback(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(ratingKeys.feedbacks.detail(variables.id), data);
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
