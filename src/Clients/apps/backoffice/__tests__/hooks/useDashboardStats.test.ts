import { type PropsWithChildren, createElement } from "react";

import { QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import dashboardApiClient from "@workspace/api-client/ordering/dashboard";
import type { Dashboard } from "@workspace/types/ordering/dashboard";

import { createTestQueryClient } from "@/__tests__/utils/test-utils";
import { useDashboardStats } from "@/hooks/useDashboardStats";

vi.mock("@workspace/api-client/ordering/dashboard", () => ({
  default: { get: vi.fn(), subscribe: vi.fn() },
}));

const account = vi.hoisted(() => ({ id: "test-admin" }));
vi.mock("@/hooks/useUserContext", () => ({
  useUserContext: () => ({ user: { id: account.id } }),
}));

const snapshot: Dashboard = {
  totalOrders: 40,
  totalRevenue: 250,
  totalCustomers: 30,
  totalBooks: 35,
  dailyOrders: [{ date: "2026-10-04", orders: 3, revenue: 20 }],
  categories: [{ name: "Fiction", value: 35 }],
  recentOrders: [],
  updatedAt: "2026-10-04T10:00:00Z",
};

function createWrapper() {
  const client = createTestQueryClient();
  return function DashboardTestProvider({ children }: PropsWithChildren) {
    return createElement(QueryClientProvider, { client }, children);
  };
}

describe("useDashboardStats", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    account.id = "test-admin";
    vi.mocked(dashboardApiClient.get).mockResolvedValue(snapshot);
    vi.mocked(dashboardApiClient.subscribe).mockReturnValue(vi.fn());
  });

  it("loads global metrics and opens a single subscription", async () => {
    const { result, rerender } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });
    expect(result.current.isLoading).toBe(true);
    await waitFor(() => expect(result.current.data).toEqual(snapshot));
    await waitFor(() =>
      expect(dashboardApiClient.subscribe).toHaveBeenCalledTimes(1),
    );
    rerender();
    expect(dashboardApiClient.subscribe).toHaveBeenCalledTimes(1);
    expect(dashboardApiClient.get).toHaveBeenCalledTimes(1);
  });

  it("keeps the last snapshot during disconnects and replaces it on reconnect", async () => {
    const { result } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });
    await waitFor(() =>
      expect(dashboardApiClient.subscribe).toHaveBeenCalled(),
    );
    const [onSnapshot, onDisconnect] = vi.mocked(dashboardApiClient.subscribe)
      .mock.calls[0]!;
    act(onDisconnect);
    expect(result.current.isDisconnected).toBe(true);
    expect(result.current.data).toEqual(snapshot);
    const updated = {
      ...snapshot,
      totalOrders: 41,
      updatedAt: "2026-10-04T10:01:00Z",
    };
    act(() => onSnapshot(updated));
    await waitFor(() => expect(result.current.data).toEqual(updated));
    expect(result.current.isDisconnected).toBe(false);
    act(() => onSnapshot(snapshot));
    expect(result.current.data?.totalOrders).toBe(41);
  });

  it("closes the subscription when the dashboard unmounts", async () => {
    const unsubscribe = vi.fn();
    vi.mocked(dashboardApiClient.subscribe).mockReturnValue(unsubscribe);
    const { unmount } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });
    await waitFor(() =>
      expect(dashboardApiClient.subscribe).toHaveBeenCalled(),
    );
    unmount();
    expect(unsubscribe).toHaveBeenCalledOnce();
  });

  it("exposes initial failures without zero-valued dashboard data", async () => {
    vi.mocked(dashboardApiClient.get).mockRejectedValue(
      new Error("Unavailable"),
    );
    const { result } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.data).toBeUndefined();
    expect(dashboardApiClient.subscribe).not.toHaveBeenCalled();
  });

  it("does not reuse another account's dashboard data", async () => {
    const unsubscribe = vi.fn();
    vi.mocked(dashboardApiClient.subscribe).mockReturnValue(unsubscribe);
    const { result, rerender } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });
    await waitFor(() => expect(result.current.data).toEqual(snapshot));
    await waitFor(() =>
      expect(dashboardApiClient.subscribe).toHaveBeenCalled(),
    );
    vi.mocked(dashboardApiClient.get).mockRejectedValue(new Error("Forbidden"));
    account.id = "different-account";
    rerender();
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.data).toBeUndefined();
    expect(unsubscribe).toHaveBeenCalledOnce();
  });
});
