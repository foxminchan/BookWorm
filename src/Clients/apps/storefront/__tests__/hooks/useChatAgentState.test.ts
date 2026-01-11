import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useChatAgentState } from "@/hooks/useChatAgentState";

const mockSetState = vi.fn();

vi.mock("@copilotkit/react-core", () => ({
  useCoAgent: vi.fn(() => ({
    state: {
      searchQuery: "initial",
      searchResults: [],
      lastAction: "start",
      conversationContext: "context",
    },
    setState: mockSetState,
    running: true,
    nodeName: "node-1",
    threadId: "thread-123",
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
    expect(result.current.currentNode).toBe("node-1");
    expect(result.current.threadId).toBe("thread-123");

    result.current.setState({ searchQuery: "updated" });
    expect(mockSetState).toHaveBeenCalledWith({ searchQuery: "updated" });
  });
});
