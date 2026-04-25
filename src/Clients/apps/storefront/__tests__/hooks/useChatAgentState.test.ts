import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useChatAgentState } from "@/hooks/useChatAgentState";

vi.mock("@copilotkit/react-core/v2", () => ({
  useAgent: vi.fn(() => ({
    agent: {
      state: {
        searchQuery: "initial",
        searchResults: [],
        lastAction: "start",
        conversationContext: "context",
      },
      isRunning: true,
      threadId: "thread-123",
    },
  })),
}));

vi.mock("@/env.mjs", () => ({
  env: {
    NEXT_PUBLIC_COPILOT_AGENT_NAME: "test-agent",
  },
}));

describe("useChatAgentState", () => {
  it("returns agent state and metadata", () => {
    const { result } = renderHook(() => useChatAgentState());

    expect(result.current.state.searchQuery).toBe("initial");
    expect(result.current.isAgentRunning).toBe(true);
    expect(result.current.threadId).toBe("thread-123");
  });
});
