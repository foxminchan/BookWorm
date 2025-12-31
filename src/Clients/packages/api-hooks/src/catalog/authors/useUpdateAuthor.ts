import {
  useMutation,
  useQueryClient,
  type UseMutationOptions,
} from "@tanstack/react-query";
import authorsApiClient from "@workspace/api-client/catalog/authors";
import type {
  Author,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { catalogKeys } from "../../keys";

export default function useUpdateAuthor(
  options?: UseMutationOptions<Author, Error, { request: UpdateAuthorRequest }>,
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ request }) => authorsApiClient.update(request),
    onSuccess: (data, variables) => {
      queryClient.setQueryData(
        catalogKeys.authors.detail(variables.request.id),
        data,
      );
      queryClient.invalidateQueries({ queryKey: catalogKeys.authors.lists() });
    },
    ...options,
  });
}
