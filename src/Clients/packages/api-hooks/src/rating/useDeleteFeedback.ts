"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import ratingApiClient from "@workspace/api-client/rating/feedbacks";

import { ratingKeys } from "../keys";
import { useMutationFromOptions } from "../use-mutation-from-options";

export default function useDeleteFeedback(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => ratingApiClient.delete(id),
      onSuccess: (_, id) => {
        queryClient.removeQueries({
          queryKey: ratingKeys.feedbacks.detail(id),
        });
        queryClient.invalidateQueries({
          queryKey: ratingKeys.feedbacks.lists(),
        });
      },
    },
    options,
  );
}
