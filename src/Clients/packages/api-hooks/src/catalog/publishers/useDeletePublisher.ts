import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import { catalogKeys } from "../../keys";

export default function useDeletePublisher(
  options?: UseMutationOptions<void, Error, string>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id) => publishersApiClient.delete(id),
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
