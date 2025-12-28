import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { authorsApiClient } from "@workspace/api-client";
import type {
  Author,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export function useUpdateAuthor(
  options?: UseMutationOptions<
    Author,
    Error,
    { id: string; request: UpdateAuthorRequest }
  >,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, request }) => authorsApiClient.updateAuthor(id, request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(catalogKeys.authors.detail(variables.id), data);
      queryClient.invalidateQueries({ queryKey: catalogKeys.authors.lists() });
    },
    ...options,
  });
}
