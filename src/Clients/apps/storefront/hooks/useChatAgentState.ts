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
 * Hook for reading shared agent state from the chat agent.
 * In v2, state is owned by the AbstractAgent instance returned by useAgent.
 * The state is cast to ChatAgentState for typed access in the UI.
 */
export function useChatAgentState() {
  const agentId = env.NEXT_PUBLIC_COPILOT_AGENT_NAME;

  const { agent } = useAgent({ agentId });

  return {
    state: agent.state as ChatAgentState,
    isAgentRunning: agent.isRunning,
    threadId: agent.threadId,
  };
}
