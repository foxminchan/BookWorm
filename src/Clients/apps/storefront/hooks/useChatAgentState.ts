"use client";

import { useAgent } from "@copilotkit/react-core/v2";

import { env } from "@/env.mjs";

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
};

/**
 * Hook for bidirectional state synchronization with the chat agent
 * Provides setState and state for shared agent/UI state management
 */
export function useChatAgentState() {
  const agentName = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  const { agent } = useAgent({
    agentId: agentName,
  });

  const state = (agent.state as ChatAgentState | undefined) ?? {
    searchQuery: undefined,
    searchResults: undefined,
    lastAction: undefined,
    conversationContext: undefined,
  };

  return {
    state,
    setState: agent.setState,
    isAgentRunning: agent.isRunning,
    currentNode: undefined,
    threadId: agent.threadId,
  };
}
