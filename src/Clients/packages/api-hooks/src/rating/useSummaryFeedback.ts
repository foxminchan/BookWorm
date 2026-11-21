"use client";

import type { UseQueryOptions } from "@tanstack/react-query";

import type { FeedbackSummary } from "@workspace/types/rating";

import { useQueryFromOptions } from "../use-query-from-options";
import { feedbackQueries } from "./queries";

export default function useSummaryFeedback(
  bookId: string,
  options?: Omit<UseQueryOptions<FeedbackSummary>, "queryKey" | "queryFn">,
) {
  return useQueryFromOptions(feedbackQueries.summary(bookId), options);
}
