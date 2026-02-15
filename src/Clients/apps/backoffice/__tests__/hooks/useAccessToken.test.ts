import { renderHook } from "@testing-library/react";
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
  signOut: vi.fn().mockResolvedValue(undefined),
  signIn: vi.fn(),
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

  it("should return null from provider when there is no session user", async () => {
    mockUseSession.mockReturnValue({ data: null });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock.calls[0]![0] as () =>
      | Promise<string | null>
      | string
      | null;
    const token = await tokenProvider();

    expect(token).toBeNull();
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

    const tokenProvider = mockSetTokenProvider.mock
      .calls[0]![0] as () => Promise<string | null>;
    const token = await tokenProvider();

    expect(token).toBe("tok_abc");
    expect(mockGetAccessToken).toHaveBeenCalledWith({
      providerId: "keycloak",
    });
  });

  it("should return null when getAccessToken returns no data", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockResolvedValue({ data: null });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock
      .calls[0]![0] as () => Promise<string | null>;
    const token = await tokenProvider();

    expect(token).toBeNull();
  });

  it("should return null when getAccessToken throws", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken.mockRejectedValue(new Error("network error"));

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock
      .calls[0]![0] as () => Promise<string | null>;
    const token = await tokenProvider();

    expect(token).toBeNull();
  });

  it("should re-register provider when session user changes", () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Alice" } },
    });

    const { rerender } = renderHook(() => useAccessToken(), {
      wrapper: createWrapper(),
    });

    expect(mockSetTokenProvider).toHaveBeenCalledTimes(1);

    mockUseSession.mockReturnValue({
      data: { user: { id: "u2", name: "Bob" } },
    });

    rerender();

    expect(mockSetTokenProvider).toHaveBeenCalledTimes(2);
  });

  it("should return fresh token on each provider call (auto-refresh)", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Test" } },
    });
    mockGetAccessToken
      .mockResolvedValueOnce({ data: { accessToken: "tok_first" } })
      .mockResolvedValueOnce({ data: { accessToken: "tok_refreshed" } });

    renderHook(() => useAccessToken(), { wrapper: createWrapper() });

    const tokenProvider = mockSetTokenProvider.mock
      .calls[0]![0] as () => Promise<string | null>;

    expect(await tokenProvider()).toBe("tok_first");
    expect(await tokenProvider()).toBe("tok_refreshed");
    expect(mockGetAccessToken).toHaveBeenCalledTimes(2);
  });

  it("should return null after user logs out", async () => {
    mockUseSession.mockReturnValue({
      data: { user: { id: "u1", name: "Alice" } },
    });
    mockGetAccessToken.mockResolvedValue({
      data: { accessToken: "tok_alice" },
    });

    const { rerender } = renderHook(() => useAccessToken(), {
      wrapper: createWrapper(),
    });

    // First provider resolves with token
    const provider1 = mockSetTokenProvider.mock.calls[0]![0] as () => Promise<
      string | null
    >;
    expect(await provider1()).toBe("tok_alice");

    // User logs out
    mockUseSession.mockReturnValue({ data: null });
    rerender();

    // New provider registered after logout
    const provider2 = mockSetTokenProvider.mock.calls.at(
      -1,
    )![0] as () => Promise<string | null>;
    expect(await provider2()).toBeNull();
  });
});
