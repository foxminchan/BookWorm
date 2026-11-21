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
      queryFn: () => booksApiClient.list(query),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.books.detail(id),
      queryFn: () => booksApiClient.get(id),
    }),
};

export const authorQueries = {
  all: () => catalogKeys.authors.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.authors.list(),
      queryFn: () => authorsApiClient.list(),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.authors.detail(id),
      queryFn: () => authorsApiClient.get(id),
    }),
};

export const categoryQueries = {
  all: () => catalogKeys.categories.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.categories.list(),
      queryFn: () => categoriesApiClient.list(),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.categories.detail(id),
      queryFn: () => categoriesApiClient.get(id),
    }),
};

export const publisherQueries = {
  all: () => catalogKeys.publishers.all,
  list: () =>
    queryOptions({
      queryKey: catalogKeys.publishers.list(),
      queryFn: () => publishersApiClient.list(),
    }),
  detail: (id: string) =>
    queryOptions({
      queryKey: catalogKeys.publishers.detail(id),
      queryFn: () => publishersApiClient.get(id),
    }),
};
