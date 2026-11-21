import { queryOptions } from "@tanstack/react-query";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import booksApiClient from "@workspace/api-client/catalog/books";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import type { ListBooksQuery } from "@workspace/types/catalog/books";

import { catalogKeys } from "../keys";

export const bookQueries = {
  all: () => catalogKeys.books.all,
  list: (query?: ListBooksQuery) =>
    queryOptions({
      queryKey: catalogKeys.books.list(query),
      queryFn: ({ signal }) => booksApiClient.list(query, signal),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.books.detail(id),
      queryFn: ({ signal }) => booksApiClient.get(id, signal),
    }),
};

export const authorQueries = {
  all: () => catalogKeys.authors.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.authors.list(),
      queryFn: ({ signal }) => authorsApiClient.list(signal),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.authors.detail(id),
      queryFn: ({ signal }) => authorsApiClient.get(id, signal),
    }),
};

export const categoryQueries = {
  all: () => catalogKeys.categories.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.categories.list(),
      queryFn: ({ signal }) => categoriesApiClient.list(signal),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.categories.detail(id),
      queryFn: ({ signal }) => categoriesApiClient.get(id, signal),
    }),
};

export const publisherQueries = {
  all: () => catalogKeys.publishers.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.publishers.list(),
      queryFn: ({ signal }) => publishersApiClient.list(signal),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.publishers.detail(id),
      queryFn: ({ signal }) => publishersApiClient.get(id, signal),
    }),
};
