"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import publishersApiClient from "@workspace/api-client/catalog/publishers";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useDeletePublisher(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (id) => publishersApiClient.delete(id),
      onSuccess: (_, id) => {
        queryClient.removeQueries({
          queryKey: catalogKeys.publishers.detail(id),
        });
        queryClient.invalidateQueries({
          queryKey: catalogKeys.publishers.lists(),
        });
      },
    },
    options,
  );
}
