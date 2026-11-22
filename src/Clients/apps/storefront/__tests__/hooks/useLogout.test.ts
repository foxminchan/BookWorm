import { renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { authClient } from "@workspace/storefront-shell/auth-client";
import { useLogout } from "@workspace/storefront-shell/hooks/useLogout";

// Mock next/navigation
const mockPush = vi.fn();
vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock auth-client
vi.mock("@workspace/storefront-shell/auth-client", () => ({
  authClient: {
    signOut: vi.fn(),
  },
}));

const mockSignOut = authClient.signOut as ReturnType<typeof vi.fn>;
const mockAssign = vi.fn();

describe("useLogout", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockSignOut.mockResolvedValue({});
    vi.stubGlobal("location", { ...globalThis.location, assign: mockAssign });
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("should return logout function and isLoggingOut state", () => {
    const { result } = renderHook(() => useLogout());

    expect(result.current.logout).toBeDefined();
    expect(typeof result.current.logout).toBe("function");
    expect(result.current.isLoggingOut).toBe(false);
  });

  it("should set isLoggingOut to true during logout", async () => {
    mockSignOut.mockImplementation(
      ({ fetchOptions }) =>
        new Promise((resolve) => {
          setTimeout(() => {
            fetchOptions?.onSuccess?.();
            resolve({});
          }, 100);
        }),
    );

    const { result } = renderHook(() => useLogout());

    const logoutPromise = result.current.logout();

    await waitFor(() => {
      expect(mockSignOut).toHaveBeenCalled();
    });

    await logoutPromise;
  });

  it("should call authClient.signOut when logout is called", async () => {
    mockSignOut.mockImplementation(({ fetchOptions }) => {
      fetchOptions?.onSuccess?.();
      return Promise.resolve({});
    });

    const { result } = renderHook(() => useLogout());

    await result.current.logout();

    expect(mockSignOut).toHaveBeenCalledTimes(1);
    expect(mockSignOut).toHaveBeenCalledWith({
      fetchOptions: {
        onSuccess: expect.any(Function),
      },
    });
  });

  it("should perform cross-zone hard navigation to home page after successful logout", async () => {
    mockSignOut.mockImplementation(({ fetchOptions }) => {
      fetchOptions?.onSuccess?.();
      return Promise.resolve({});
    });

    const { result } = renderHook(() => useLogout());

    await result.current.logout();

    await waitFor(() => {
      expect(mockAssign).toHaveBeenCalledWith("/");
    });
    expect(mockPush).not.toHaveBeenCalled();
  });

  it("should log error and reset isLoggingOut when signOut fails", async () => {
    const consoleSpy = vi.spyOn(console, "error").mockImplementation(() => {});
    const error = new Error("Sign out failed");
    mockSignOut.mockRejectedValue(error);

    const { result } = renderHook(() => useLogout());

    await result.current.logout();

    expect(consoleSpy).toHaveBeenCalledWith("Logout failed:", error);
    expect(result.current.isLoggingOut).toBe(false);
    consoleSpy.mockRestore();
  });
});
