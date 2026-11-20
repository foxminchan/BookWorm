"use client";

import { useConfigureSuggestions } from "@copilotkit/react-core/v2";

import { env } from "@/env.mjs";

/**
 * Configures starter and contextual chat suggestions for the BookWorm
 * literary assistant across book discovery, basket, and policy support.
 */
export function useChatSuggestions() {
  useConfigureSuggestions(
    {
      consumerAgentId: env.NEXT_PUBLIC_COPILOT_AGENT_NAME,
      suggestions: [
        {
          title: "Find sci-fi books",
          message: "Recommend some popular science fiction books in stock.",
        },
        {
          title: "View my basket",
          message: "What is currently in my shopping basket?",
        },
        {
          title: "Return policy",
          message: "What is BookWorm's return and refund policy?",
        },
      ],
      available: "before-first-message",
    },
    [],
  );

  useConfigureSuggestions(
    {
      consumerAgentId: env.NEXT_PUBLIC_COPILOT_AGENT_NAME,
      instructions:
        "Suggest 2-3 concise next steps for exploring books, checking reviews, or managing the customer's basket.",
      available: "after-first-message",
    },
    [],
  );
}
