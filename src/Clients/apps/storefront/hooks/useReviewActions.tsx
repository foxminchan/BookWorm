"use client";

import { useEffect, useRef } from "react";

import { useCopilotAction } from "@copilotkit/react-core";

import { ReviewFormWidget } from "@/components/chat-hitl/ReviewFormWidget";

import { useChatAgentState } from "./useChatAgentState";

/**
 * Provides CopilotKit action for chat-based review submission with HITL approval
 * via `renderAndWaitForResponse`. Includes cancel-on-new-message behavior.
 */
export function useReviewActions() {
  const { isAgentRunning } = useChatAgentState();

  const pendingRespond = useRef<
    | ((response: {
        approved: boolean;
        rating?: number;
        comment?: string;
      }) => void)
    | null
  >(null);

  // Cancel pending review form when agent starts processing a new message
  useEffect(() => {
    if (isAgentRunning && pendingRespond.current) {
      pendingRespond.current({ approved: false });
      pendingRespond.current = null;
    }
  }, [isAgentRunning]);

  useCopilotAction({
    name: "submitReview",
    description:
      "Submit a product review and star rating for a book. Always requires explicit customer confirmation before submitting.",
    parameters: [
      {
        name: "bookId",
        type: "string",
        description: "The unique identifier of the book to review",
        required: true,
      },
      {
        name: "bookTitle",
        type: "string",
        description: "The title of the book (for display in the review form)",
        required: false,
      },
      {
        name: "initialRating",
        type: "number",
        description: "Suggested star rating (1–5) to pre-fill the form",
        required: false,
      },
      {
        name: "initialComment",
        type: "string",
        description: "Suggested comment text to pre-fill the form",
        required: false,
      },
    ],
    renderAndWaitForResponse({ args, respond, status }) {
      const { bookId, bookTitle, initialRating, initialComment } = args;

      // Store the respond callback for cancel-on-new-message
      if (status === "inProgress" && respond) {
        pendingRespond.current = respond;
      }

      if (status === "complete") {
        return <></>;
      }

      return (
        <ReviewFormWidget
          bookId={String(bookId ?? "")}
          bookTitle={bookTitle != null ? String(bookTitle) : undefined}
          initialRating={
            initialRating != null ? Number(initialRating) : undefined
          }
          initialComment={
            initialComment != null ? String(initialComment) : undefined
          }
          onConfirm={(rating, comment) => {
            pendingRespond.current = null;
            respond?.({ approved: true, rating, comment });
          }}
          onDismiss={() => {
            pendingRespond.current = null;
            respond?.({ approved: false });
          }}
        />
      );
    },
  });
}
