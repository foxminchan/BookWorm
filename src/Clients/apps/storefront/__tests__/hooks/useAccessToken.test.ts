import { act, renderHook } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { createWrapper } from "@/__tests__/utils/test-utils";
import { useAccessToken } from "@/hooks/useAccessToken";

// ---------- mocks ----------

const mockSetTokenProvider = vi.fn();
vi.mock("@workspace/api-client/client", () => ({
  apiClient: {
    setTokenProvider: (...args: unknown[]) => mockSetTokenProvider(...args),
  },
}));

const mockUseSession = vi.fn();
const mockGetAccessToken = vi.fn();
vi.mock("@/lib/auth-client", () => ({
  useSession: () => mockUseSession(),
  authClient: {
    getAccessToken: (...args: unknown[]) => mockGetAccessToken(...args),
  },
}));

// ---------- tests ----------

describe("useAccessToken", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    mockUseSession.mockReturnValue({ data: null });
    mockGetAccessToken.mockResolvedValue({ data: null });
  });

  it("should register a token provider with apiClient on mount", () => {
    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    expect(mockSetTokenProvider).toHaveBeenCalledTimes(1);
    expect(mockSetTokenProvider).toHaveBeenCalledWith(expect.any(Function));
  });

  it("should not fetch token when there is no session user", () => {
    mockUseSession.mockReturnValue({ data: null });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    expect(mockGetAccessToken).not.toHaveBeenCalled();
  });

  it("should fetch token when session user is present", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_abc" },
    });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    await vi.waitFor(() => {
      expect(mockGetAccessToken).toHaveBeenCalledWith({
        providerId: "keycloak",
      });
    });
  });

  it("should expose the fetched token through the provider callback", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_xyz" },
    });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | string
      | null;

    await vi.waitFor(() => {
      expect(tokenProvider()).toBe("tok_xyz");
    });
  });

  it("should set token to null when getAccessToken returns no data", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockResolvedValue({ data: null });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | string
      | null;

    await vi.waitFor(() => {
      expect(mockGetAccessToken).toHaveBeenCalled();
    });

    expect(tokenProvider()).toBeNull();
  });

  it("should set token to null when getAccessToken throws", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockRejectedValue(new Error("network error"));

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | string
      | null;

    await vi.waitFor(() => {
      expect(mockGetAccessToken).toHaveBeenCalled();
    });

    expect(tokenProvider()).toBeNull();
  });

  it("should refetch token when session user changes", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Alice" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_alice" },
    });

    const { rerender } = renderHook(() => useAccessToken(), {
      wrapper: createWrapper(),
    });

    await vi.waitFor(() => {
      expect(mockGetAccessToken).toHaveBeenCalledTimes(1);
    });

    mockUseSession.mockReturnValue({
      data: { user: { id: "u2", name: "Bob" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_bob" },
    });

    act(() => {
      rerender();
    });

    await vi.waitFor(() => {
      expect(mockGetAccessToken).toHaveBeenCalledTimes(2);
    });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | string
      | null;
    expect(tokenProvider()).toBe("tok_bob");
  });

  it("should clear token when user logs out", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Alice" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_alice" },
    });

    const { rerender } = renderHook(() => useAccessToken(), {
      wrapper: createWrapper(),
    });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | string
      | null;

    await vi.waitFor(() => {
      expect(tokenProvider()).toBe("tok_alice");
    });

    mockUseSession.mockReturnValue({ data: null });

    act(() => {
      rerender();
    });

    await vi.waitFor(() => {
      expect(tokenProvider()).toBeNull();
    });
  });
});
