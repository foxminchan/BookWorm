import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { publishersApiClient } from "@workspace/api-client";
import { catalogKeys } from "../../keys";

export function useDeletePublisher(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => publishersApiClient.deletePublisher(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({
        queryKey: catalogKeys.publishers.detail(id),
      });
      queryClient.invalidateQueries({
        queryKey: catalogKeys.publishers.lists(),
      });
    },
    ...options,
  });
}
