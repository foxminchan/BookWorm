"use client";

import { type UseMutationOptions, useQueryClient } from "@tanstack/react-query";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import type {
  Author,
  CreateAuthorRequest,
} from "@workspace/types/catalog/authors";

import { catalogKeys } from "../../keys";
import { useMutationFromOptions } from "../../use-mutation-from-options";

export default function useCreateAuthor(
  options?: UseMutationOptions<Author, Error, CreateAuthorRequest>,
) {
  const queryClient = useQueryClient();

  return useMutationFromOptions(
    {
      mutationFn: (request) => authorsApiClient.create(request),
      onSuccess: () => {
        queryClient.invalidateQueries({
          queryKey: catalogKeys.authors.lists(),
        });
      },
    },
    options,
  );
}
