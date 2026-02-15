import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createWrapper } from "@/__tests__/utils/test-utils";
import { useUserContext } from "@/hooks/useUserContext";

// Mock useSession from auth-client
const mockUseSession = vi.hoisted(() => vi.fn());
vi.mock("@/lib/auth-client", () => ({
  useSession: () => mockUseSession(),
  signOut: vi.fn().mockResolvedValue(undefined),
  signIn: { social: vi.fn() },
  authClient: {
    getAccessToken: vi.fn().mockResolvedValue({ data: null }),
    signOut: vi.fn().mockResolvedValue(undefined),
  },
}));

describe("useUserContext", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should return loading state when session is pending", () => {
    mockUseSession.mockReturnValue({
      data: null,
      isPending: true,
      error: null,
    });

    const { result } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it("should return authenticated user when session exists", () => {
    const mockUser = {
      id: "user-1",
      name: "Test User",
      email: "test@example.com",
    };

    mockUseSession.mockReturnValue({
      data: { user: mockUser },
      isPending: false,
      error: null,
    });

    const { result } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isAuthenticated).toBe(true);
    expect(result.current.user).toEqual(mockUser);
    expect(result.current.error).toBeNull();
  });

  it("should return not authenticated when session is null", () => {
    mockUseSession.mockReturnValue({
      data: null,
      isPending: false,
      error: null,
    });

    const { result } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toBeNull();
  });

  it("should return error when session fails", () => {
    const mockError = new Error("Session failed");

    mockUseSession.mockReturnValue({
      data: null,
      isPending: false,
      error: mockError,
    });

    const { result } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
    expect(result.current.error).toEqual(mockError);
  });

  it("should handle session without user property", () => {
    mockUseSession.mockReturnValue({
      data: {},
      isPending: false,
      error: null,
    });

    const { result } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.isAuthenticated).toBe(false);
    expect(result.current.user).toBeNull();
  });

  it("should transition from loading to authenticated", async () => {
    const mockUser = {
      id: "user-1",
      name: "Test User",
      email: "test@example.com",
    };

    // Start with loading
    mockUseSession.mockReturnValue({
      data: null,
      isPending: true,
      error: null,
    });

    const { result, rerender } = renderHook(() => useUserContext(), {
      wrapper: createWrapper(),
    });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.isAuthenticated).toBe(false);

    // Transition to authenticated
    mockUseSession.mockReturnValue({
      data: { user: mockUser },
      isPending: false,
      error: null,
    });

    rerender();

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.isAuthenticated).toBe(true);
      expect(result.current.user).toEqual(mockUser);
    });
  });
});
