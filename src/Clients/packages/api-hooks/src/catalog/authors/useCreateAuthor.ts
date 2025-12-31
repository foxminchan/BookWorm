import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import authorsApiClient from "@workspace/api-client/catalog/authors";
import type {
  Author,
  CreateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export default function useCreateAuthor(
  options?: UseMutationOptions<Author, Error, CreateAuthorRequest>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (request) => authorsApiClient.create(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: catalogKeys.authors.lists() });
    },
    ...options,
  });
}
