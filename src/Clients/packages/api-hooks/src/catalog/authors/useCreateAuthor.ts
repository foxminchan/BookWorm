import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import { authorsApiClient } from "@workspace/api-client";
import type {
  Author,
  CreateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export function useCreateAuthor(
  options?: UseMutationOptions<Author, Error, CreateAuthorRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => authorsApiClient.createAuthor(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.authors.lists() });
    },
    ...options,
  });
}
