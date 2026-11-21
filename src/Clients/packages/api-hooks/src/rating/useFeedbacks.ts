"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { Feedback, ListFeedbacksQuery } from "@workspace/types/rating";
import type { PagedResult } from "@workspace/types/shared";

import { useQueryFromOptions } from "../use-query-from-options";
import { feedbackQueries } from "./queries";

export default function useFeedbacks(
  query?: ListFeedbacksQuery,
  options?: Omit<
    UseQueryOptions<PagedResult<Feedback>>,
    "queryKey" | "queryFn"
  >,
) {
  return useQueryFromOptions(feedbackQueries.list(query), options);
}
