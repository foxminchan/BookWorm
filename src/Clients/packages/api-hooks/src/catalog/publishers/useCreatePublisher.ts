import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { publishersApiClient } from "@workspace/api-client";
import type {
  Publisher,
  CreatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import { catalogKeys } from "../../keys";

export function useCreatePublisher(
  options?: UseMutationOptions<Publisher, Error, CreatePublisherRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => publishersApiClient.createPublisher(request),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: catalogKeys.publishers.lists(),
      });
    },
    ...options,
  });
}
