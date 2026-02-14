import { act, renderHook } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import { useOfflineQueue } from "@/hooks/useOfflineQueue";

const setNavigatorOnline = (value: boolean) => {
  Object.defineProperty(globalThis.navigator, "onLine", {
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
      globalThis.dispatchEvent(new Event("online"));
    });

    const syncPromise = act(async () => {
      const pending = result.current.syncQueue();
      if (queuedId) {
        globalThis.dispatchEvent(
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
      globalThis.dispatchEvent(new Event("offline"));
    });
    expect(result.current.isOnline).toBe(false);

    act(() => {
      globalThis.dispatchEvent(new Event("online"));
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
      globalThis.dispatchEvent(new Event("online"));
    });

    expect(result.current.isOnline).toBe(true);
    vi.useRealTimers();
  });

  it("sends message successfully when online", async () => {
    setNavigatorOnline(true);
    const sendFn = vi.fn().mockResolvedValue(undefined);
    const { result } = renderHook(() => useOfflineQueue());

    let response: { success: boolean; queued: boolean } | undefined;
    await act(async () => {
      response = await result.current.sendMessage("hello", sendFn);
    });

    expect(response).toEqual({ success: true, queued: false });
    expect(sendFn).toHaveBeenCalledWith("hello");
    expect(result.current.queueSize).toBe(0);
  });

  it("skips sync when already syncing", async () => {
    // Pre-load a message so the queue is non-empty on mount
    const stored = [
      { id: "sync-msg", content: "hi", timestamp: 1, retryCount: 0 },
    ];
    localStorage.setItem("copilot-message-queue", JSON.stringify(stored));

    setNavigatorOnline(true);
    const { result } = renderHook(() => useOfflineQueue());

    // Wait for localStorage load
    await act(async () => {});

    // Start syncing — don't await so that isSyncing remains true
    act(() => {
      result.current.syncQueue();
    });

    expect(result.current.isSyncing).toBe(true);

    // Second call should be a no-op because isSyncing is true
    await act(async () => {
      await result.current.syncQueue();
    });

    // Queue should still contain the message (second sync was skipped)
    expect(result.current.queueSize).toBe(1);
  });

  it("skips sync when queue is empty", async () => {
    setNavigatorOnline(true);
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.syncQueue();
    });

    // No errors, just a no-op
    expect(result.current.isSyncing).toBe(false);
  });

  it("skips sync when offline", async () => {
    setNavigatorOnline(false);
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("msg", vi.fn());
    });

    // Don't go online — sync while offline
    await act(async () => {
      await result.current.syncQueue();
    });

    // Message should remain in queue
    expect(result.current.queueSize).toBe(1);
  });

  it("removes message from queue after max retries on failed sync", async () => {
    // Pre-load a message with retryCount at MAX_RETRIES
    const stored = [
      { id: "retry-msg", content: "hi", timestamp: 1, retryCount: 3 },
    ];
    localStorage.setItem("copilot-message-queue", JSON.stringify(stored));

    setNavigatorOnline(true);
    const { result } = renderHook(() => useOfflineQueue());

    // Wait for localStorage load
    await act(async () => {});

    await act(async () => {
      const pending = result.current.syncQueue();

      // Dispatch a failure confirmation
      globalThis.dispatchEvent(
        new CustomEvent("copilot-message-sent", {
          detail: { id: "retry-msg", success: false },
        }),
      );

      await pending;
    });

    expect(result.current.queueSize).toBe(0);
  });

  it("queues multiple messages offline", async () => {
    setNavigatorOnline(false);
    const { result } = renderHook(() => useOfflineQueue());

    await act(async () => {
      await result.current.sendMessage("msg1", vi.fn());
    });
    await act(async () => {
      await result.current.sendMessage("msg2", vi.fn());
    });

    expect(result.current.queueSize).toBe(2);
    expect(result.current.queue[0]!.content).toBe("msg1");
    expect(result.current.queue[1]!.content).toBe("msg2");
  });

  it("cleans up event listeners on unmount", () => {
    const removeEventListenerSpy = vi.spyOn(globalThis, "removeEventListener");

    const { unmount } = renderHook(() => useOfflineQueue());

    unmount();

    expect(removeEventListenerSpy).toHaveBeenCalledWith(
      "online",
      expect.any(Function),
    );
    expect(removeEventListenerSpy).toHaveBeenCalledWith(
      "offline",
      expect.any(Function),
    );

    removeEventListenerSpy.mockRestore();
  });
});
