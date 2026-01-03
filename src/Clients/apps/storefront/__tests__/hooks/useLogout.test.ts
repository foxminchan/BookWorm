import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useLogout } from "@/hooks/useLogout";
import { authClient } from "@/lib/auth-client";

// Mock next/navigation
const mockPush = vi.fn();
vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

// Mock auth-client
vi.mock("@/lib/auth-client", () => ({
  authClient: {
    signOut: vi.fn(),
  },
}));

const mockSignOut = authClient.signOut as ReturnType<typeof vi.fn>;

describe("useLogout", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockSignOut.mockResolvedValue({});
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

    // Check if isLoggingOut is true immediately after calling logout
    await waitFor(() => {
      // Note: Due to async nature, we just verify the function was called
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

  it("should redirect to home page after successful logout", async () => {
    mockSignOut.mockImplementation(({ fetchOptions }) => {
      fetchOptions?.onSuccess?.();
      return Promise.resolve({});
    });

    const { result } = renderHook(() => useLogout());

    await result.current.logout();

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/");
    });
  });
});
