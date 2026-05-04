import { renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useUserContext } from "@/hooks/useUserContext";

const mockUseAgentContext = vi.fn();
vi.mock("@copilotkit/react-core/v2", () => ({
  useAgentContext: (...args: unknown[]) => mockUseAgentContext(...args),
}));

const mockUseSession = vi.fn();
vi.mock("@/lib/auth-client", () => ({
  useSession: () => mockUseSession(),
}));

describe("useUserContext", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseSession.mockReturnValue({ data: null });
  });

  it("should register unauthenticated context when no session exists", () => {
    mockUseSession.mockReturnValue({ data: null });

    renderHook(() => useUserContext());

    expect(mockUseAgentContext).toHaveBeenCalledWith({
      description: "Whether the user is currently authenticated",
      value: {
        isAuthenticated: false,
      },
    });
  });

  it("should register authenticated context when session user is present", () => {
    mockUseSession.mockReturnValue({
      data: {
        user: {
          id: "user-1",
          name: "Alice",
          email: "alice@example.com",
        },
      },
    });

    renderHook(() => useUserContext());

    expect(mockUseAgentContext).toHaveBeenCalledWith({
      description: "Whether the user is currently authenticated",
      value: {
        isAuthenticated: true,
      },
    });
  });

  it("should update context when session changes", () => {
    mockUseSession.mockReturnValue({ data: null });

    const { rerender } = renderHook(() => useUserContext());

    expect(mockUseAgentContext).toHaveBeenLastCalledWith(
      expect.objectContaining({
        value: expect.objectContaining({ isAuthenticated: false }),
      }),
    );

    mockUseSession.mockReturnValue({
      data: {
        user: { id: "user-2", name: "Bob", email: "bob@example.com" },
      },
    });

    rerender();

    expect(mockUseAgentContext).toHaveBeenLastCalledWith(
      expect.objectContaining({
        value: expect.objectContaining({
          isAuthenticated: true,
        }),
      }),
    );
  });

  it("should handle session with null user gracefully", () => {
    mockUseSession.mockReturnValue({ data: { user: null } });

    renderHook(() => useUserContext());

    expect(mockUseAgentContext).toHaveBeenCalledWith(
      expect.objectContaining({
        value: expect.objectContaining({ isAuthenticated: false }),
      }),
    );
  });
});
