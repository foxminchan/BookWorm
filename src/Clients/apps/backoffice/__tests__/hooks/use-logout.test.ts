import * as NextNavigation from "next/navigation";

import { renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createWrapper } from "@/__tests__/utils/test-utils";
import { useLogout } from "@/hooks/use-logout";

// Mock auth client
vi.mock("@/lib/auth-client", () => ({
  signOut: vi.fn(),
}));

describe("useLogout", () => {
  const mockPush = vi.fn();
  const mockRefresh = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(NextNavigation, "useRouter").mockReturnValue({
      push: mockPush,
      refresh: mockRefresh,
      replace: vi.fn(),
      prefetch: vi.fn(),
      back: vi.fn(),
      forward: vi.fn(),
    } as any);
  });

  it("should return logout function", () => {
    const { result } = renderHook(() => useLogout(), {
      wrapper: createWrapper(),
    });

    expect(result.current.logout).toBeDefined();
    expect(typeof result.current.logout).toBe("function");
  });

  it("should call signOut and navigate to home on logout", async () => {
    const { signOut } = await import("@/lib/auth-client");

    const { result } = renderHook(() => useLogout(), {
      wrapper: createWrapper(),
    });

    await result.current.logout();

    expect(signOut).toHaveBeenCalled();
    expect(mockPush).toHaveBeenCalledWith("/");
    expect(mockRefresh).toHaveBeenCalled();
  });

  it("should handle logout errors gracefully", async () => {
    const { signOut } = await import("@/lib/auth-client");
    const consoleErrorSpy = vi
      .spyOn(console, "error")
      .mockImplementation(() => {});

    vi.mocked(signOut).mockRejectedValueOnce(new Error("Logout failed"));

    const { result } = renderHook(() => useLogout(), {
      wrapper: createWrapper(),
    });

    await result.current.logout();

    expect(consoleErrorSpy).toHaveBeenCalledWith(
      "Logout failed:",
      expect.any(Error),
    );

    consoleErrorSpy.mockRestore();
  });
});
