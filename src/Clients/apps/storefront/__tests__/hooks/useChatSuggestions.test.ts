import { renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useChatSuggestions } from "@/hooks/useChatSuggestions";

const { mockUseConfigureSuggestions } = vi.hoisted(() => ({
  mockUseConfigureSuggestions: vi.fn(),
}));

vi.mock("@copilotkit/react-core/v2", () => ({
  useConfigureSuggestions: mockUseConfigureSuggestions,
}));

vi.mock("@/env.mjs", () => ({
  env: {
    NEXT_PUBLIC_COPILOT_AGENT_NAME: "chat-workflow",
  },
}));

describe("useChatSuggestions", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("registers both starter and follow-up suggestion configs", () => {
    renderHook(() => useChatSuggestions());

    expect(mockUseConfigureSuggestions).toHaveBeenCalledTimes(2);
    expect(mockUseConfigureSuggestions).toHaveBeenNthCalledWith(
      1,
      expect.objectContaining({
        consumerAgentId: "chat-workflow",
        available: "before-first-message",
        suggestions: expect.arrayContaining([
          expect.objectContaining({ title: "Find sci-fi books" }),
          expect.objectContaining({ title: "View my basket" }),
          expect.objectContaining({ title: "Return policy" }),
        ]),
      }),
      [],
    );
    expect(mockUseConfigureSuggestions).toHaveBeenNthCalledWith(
      2,
      expect.objectContaining({
        consumerAgentId: "chat-workflow",
        available: "after-first-message",
      }),
      [],
    );
  });
});
