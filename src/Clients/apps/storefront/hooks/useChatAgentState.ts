"use client";

import { useCoAgent } from "@copilotkit/react-core";

import { env } from "@/env.mjs";

/**
 * Agent state type matching the backend chat workflow state
 */
export type ChatAgentState = {
  searchQuery?: string;
  searchResults?: Array<{
    id: string;
    title: string;
    author?: string;
    price: number;
  }>;
  lastAction?: string;
  conversationContext?: string;
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
