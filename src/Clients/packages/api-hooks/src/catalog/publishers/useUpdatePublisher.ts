import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { publishersApiClient } from "@workspace/api-client";
import type {
  Publisher,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import { catalogKeys } from "../../keys";

export function useUpdatePublisher(
  options?: UseMutationOptions<
    Publisher,
    Error,
    { id: string; request: UpdatePublisherRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) =>
      publishersApiClient.updatePublisher(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(
        catalogKeys.publishers.detail(variables.id),
        data,
      );
      queryClient.invalidateQueries({
        queryKey: catalogKeys.publishers.lists(),
      });
    },
    ...options,
  });
}
