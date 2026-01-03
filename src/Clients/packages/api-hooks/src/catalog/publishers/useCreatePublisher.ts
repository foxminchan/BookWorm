import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";

import publishersApiClient from "@workspace/api-client/catalog/publishers";
import type {
  CreatePublisherRequest,
  Publisher,
} from "@workspace/types/catalog/publishers";

import { catalogKeys } from "../../keys";

export default function useCreatePublisher(
  options?: UseMutationOptions<Publisher, Error, CreatePublisherRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => publishersApiClient.create(request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: catalogKeys.publishers.lists(),
      });
    },
    ...options,
  });
}
