import { cacheLife, cacheTag } from "next/cache";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import booksApiClient from "@workspace/api-client/catalog/books";
import categoriesApiClient from "@workspace/api-client/catalog/categories";
import publishersApiClient from "@workspace/api-client/catalog/publishers";
import feedbacksApiClient from "@workspace/api-client/rating/feedbacks";
import type { ListBooksQuery } from "@workspace/types/catalog/books";
import type { ListFeedbacksQuery } from "@workspace/types/rating";

export async function getCachedBooks(params?: ListBooksQuery) {
  "use cache";
  cacheLife("minutes");
  cacheTag("books");
  return booksApiClient.list(params);
}

export async function getCachedBook(id: string) {
  "use cache";
  cacheLife("minutes");
  cacheTag("books", `book-${id}`);
  return booksApiClient.get(id);
}

export async function getCachedCategories() {
  "use cache";
  cacheLife("hours");
  cacheTag("categories");
  return categoriesApiClient.list();
}

export async function getCachedPublishers() {
  "use cache";
  cacheLife("hours");
  cacheTag("publishers");
  return publishersApiClient.list();
}

export async function getCachedAuthors() {
  "use cache";
  cacheLife("hours");
  cacheTag("authors");
  return authorsApiClient.list();
}

export async function getCachedFeedbacks(params: ListFeedbacksQuery) {
  "use cache";
  cacheLife("minutes");
  cacheTag(`feedbacks-${params.bookId}`);
  return feedbacksApiClient.list(params);
}
