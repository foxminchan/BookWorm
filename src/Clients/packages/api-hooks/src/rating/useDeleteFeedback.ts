import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import { ratingKeys } from "../keys";

export default function useDeleteFeedback(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => ratingApiClient.delete(id),
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
