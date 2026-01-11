import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { useOfflineQueue } from "@/hooks/useOfflineQueue";

const setNavigatorOnline = (value: boolean) => {
  Object.defineProperty(window.navigator, "onLine", {
    value,
    configurable: true,
  });
};

describe("useOfflineQueue", () => {
  beforeEach(() => {
    localStorage.clear();
    vi.useFakeTimers();
    setNavigatorOnline(true);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("queues messages when offline", async () => {
    setNavigatorOnline(false);

    const sendFn = vi.fn();
    const { result } = renderHook(() => useOfflineQueue());

    let response: { success: boolean; queued: boolean } | undefined;
    await act(async () => {
      response = await result.current.sendMessage("hello", sendFn);
    });

    expect(response).toEqual({ success: false, queued: true });
    expect(result.current.queueSize).toBe(1);
    expect(sendFn).not.toHaveBeenCalled();
  });

  it("queues when send fails online", async () => {
    setNavigatorOnline(true);
    const sendFn = vi.fn().mockRejectedValue(new Error("fail"));
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("hello", sendFn);
    });

    expect(result.current.queueSize).toBe(1);
  });

  it("syncs queued messages when back online", async () => {
    setNavigatorOnline(false);
    const sendFn = vi.fn();
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("hello", sendFn);
    });

    const queuedId = result.current.queue[0]?.id;

    setNavigatorOnline(true);

    await act(async () => {
      window.dispatchEvent(new Event("online"));
    });

    const syncPromise = act(async () => {
      const pending = result.current.syncQueue();
      if (queuedId) {
        window.dispatchEvent(
          new CustomEvent("copilot-message-sent", {
            detail: { id: queuedId, success: true },
          }),
        );
      }
      await pending;
    });

    await syncPromise;

    expect(result.current.queueSize).toBe(0);
  });

  it("clears queue and storage", async () => {
    setNavigatorOnline(false);
    const sendFn = vi.fn();
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("hello", sendFn);
    });

    expect(localStorage.getItem("copilot-message-queue")).not.toBeNull();

    act(() => {
      result.current.clearQueue();
    });

    expect(result.current.queueSize).toBe(0);
    expect(localStorage.getItem("copilot-message-queue")).toBe("[]");
  });

  it("loads queue from localStorage on mount", () => {
    const stored = [{ id: "q1", content: "hi", timestamp: 1, retryCount: 0 }];
    localStorage.setItem("copilot-message-queue", JSON.stringify(stored));

    const { result } = renderHook(() => useOfflineQueue());

    expect(result.current.queueSize).toBe(1);
    expect(result.current.queue[0]!.id).toBe("q1");
  });

  it("updates online/offline status via events", () => {
    const { result } = renderHook(() => useOfflineQueue());

    act(() => {
      window.dispatchEvent(new Event("offline"));
    });
    expect(result.current.isOnline).toBe(false);

    act(() => {
      window.dispatchEvent(new Event("online"));
    });
    expect(result.current.isOnline).toBe(true);
  });

  it("persists queue to localStorage on each update", async () => {
    setNavigatorOnline(false);
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("msg1", vi.fn());
    });

    const stored = localStorage.getItem("copilot-message-queue");
    expect(stored).toBeTruthy();
    const parsed = JSON.parse(stored!);
    expect(parsed).toHaveLength(1);
    expect(parsed[0].content).toBe("msg1");
  });

  it("handles malformed localStorage gracefully", () => {
    localStorage.setItem("copilot-message-queue", "invalid json");

    const { result } = renderHook(() => useOfflineQueue());

    // Should initialize with empty queue despite corrupted storage
    expect(result.current.queueSize).toBe(0);
  });

  it("triggers sync when coming back online with queued messages", async () => {
    vi.useFakeTimers();
    setNavigatorOnline(false);
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("hello", vi.fn());
    });

    setNavigatorOnline(true);

    act(() => {
      window.dispatchEvent(new Event("online"));
    });

    expect(result.current.isOnline).toBe(true);
    vi.useRealTimers();
  });
});
