import { queryOptions } from "@tanstack/react-query";

import ratingApiClient from "@workspace/api-client/rating/feedbacks";
import type { ListFeedbacksQuery } from "@workspace/types/rating";

import { ratingKeys } from "../keys";

export const feedbackQueries = {
  all: () => ratingKeys.feedbacks.all,
  list: (query?: ListFeedbacksQuery) =>
    queryOptions({
      queryKey: ratingKeys.feedbacks.list(query),
      queryFn: ({ signal }) => ratingApiClient.list(query, signal),
    }),
  summary: (bookId: string) =>
    queryOptions({
      queryKey: ratingKeys.feedbacks.summary(bookId),
      queryFn: ({ signal }) => ratingApiClient.summary(bookId, signal),
    }),
};
