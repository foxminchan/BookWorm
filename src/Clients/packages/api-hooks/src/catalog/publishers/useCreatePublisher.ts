"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import publishersApiClient from "@workspace/api-client/catalog/publishers";
import type {
  CreatePublisherRequest,
  Publisher,
} from "@workspace/types/catalog/publishers";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCreatePublisher(
  options?: UseMutationOptions<Publisher, Error, CreatePublisherRequest>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (request) => publishersApiClient.create(request),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: catalogKeys.publishers.lists(),
        });
      },
    },
    options,
  );
}
