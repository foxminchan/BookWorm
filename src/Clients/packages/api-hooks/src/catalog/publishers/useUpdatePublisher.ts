"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import publishersApiClient from "@workspace/api-client/catalog/publishers";
import type {
  Publisher,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useUpdatePublisher(
  options?: UseMutationOptions<
    Publisher,
    Error,
    { request: UpdatePublisherRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request }) => publishersApiClient.update(request),
      onSuccess: (data, variables) => {
        queryClient.setQueryData(
          catalogKeys.publishers.detail(variables.request.id),
          data,
        );
        queryClient.invalidateQueries({
          queryKey: catalogKeys.publishers.lists(),
        });
      },
    },
    options,
  );
}
