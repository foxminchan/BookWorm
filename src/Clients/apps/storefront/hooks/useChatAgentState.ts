"use client";

import { useCoAgent } from "@copilotkit/react-core";

import { env } from "@/env.mjs";

/**
 * State shape for a pending basket add approval dialog.
 */
type BasketApprovalState = {
  approvalId: string;
  bookId: string;
  bookTitle?: string;
  quantity: number;
  unitPrice?: number;
};

/**
 * State shape for a pending review submission approval dialog.
 */
type ReviewApprovalState = {
  approvalId: string;
  bookId: string;
  bookTitle?: string;
  rating: number;
  comment?: string;
};

/**
 * Agent state type matching the backend chat workflow state
 */
type ChatAgentState = {
  searchQuery?: string;
  searchResults?: Array<{
    id: string;
    title: string;
    author?: string;
    price: number;
  }>;
  lastAction?: string;
  conversationContext?: string;
  pendingBasketApproval?: BasketApprovalState;
  pendingReviewApproval?: ReviewApprovalState;
};

/**
 * Hook for bidirectional state synchronization with the chat agent
 * Provides setState and state for shared agent/UI state management
 */
export function useChatAgentState() {
  const agentName = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  const { state, setState, running, nodeName, threadId } =
    useCoAgent<ChatAgentState>({
      name: agentName,
      initialState: {
        searchQuery: undefined,
        searchResults: undefined,
        lastAction: undefined,
        conversationContext: undefined,
        pendingBasketApproval: undefined,
        pendingReviewApproval: undefined,
      },
    });

  return {
    state,
    setState,
    isAgentRunning: running,
    currentNode: nodeName,
    threadId,
  };
}
