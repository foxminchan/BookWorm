import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { ratingApiClient } from "@workspace/api-client";
import { ratingKeys } from "../keys";

export function useDeleteFeedback(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => ratingApiClient.deleteFeedback(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({
        queryKey: ratingKeys.feedbacks.detail(id),
      });
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: ratingKeys.feedbacks.all,
      });
    },
    ...options,
  });
}
