"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import type {
  Author,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useUpdateAuthor(
  options?: UseMutationOptions<Author, Error, { request: UpdateAuthorRequest }>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: ({ request }) => authorsApiClient.update(request),
      onSuccess: (data, variables) => {
        queryClient.setQueryData(
          catalogKeys.authors.detail(variables.request.id),
          data,
        );
        queryClient.invalidateQueries({
          queryKey: catalogKeys.authors.lists(),
        });
      },
    },
    options,
  );
}
