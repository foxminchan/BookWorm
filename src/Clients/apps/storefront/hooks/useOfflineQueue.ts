"use client";

import { useCallback, useEffect, useRef, useState } from "react";

type QueuedMessage = {
  id: string;
  content: string;
  timestamp: number;
  retryCount: number;
};

type OfflineQueueState = {
  isOnline: boolean;
  queue: QueuedMessage[];
  isSyncing: boolean;
};

const MAX_RETRIES = 3;
const RETRY_DELAY = 2000;
const QUEUE_STORAGE_KEY = "copilot-message-queue";

export function useOfflineQueue() {
  const [state, setState] = useState<OfflineQueueState>({
    isOnline: typeof navigator === "undefined" ? true : navigator.onLine,
    queue: [],
    isSyncing: false,
  });
  const syncTimeoutRef = useRef<NodeJS.Timeout | undefined>(undefined);
  const syncQueueRef = useRef<(() => Promise<void>) | null>(null);

  const removeFromQueue = useCallback((messageId: string) => {
    setState((prev) => ({
      ...prev,
      queue: prev.queue.filter((msg) => msg.id !== messageId),
    }));
  }, []);

  const incrementRetryCount = useCallback((messageId: string) => {
    setState((prev) => ({
      ...prev,
      queue: prev.queue.map((msg) =>
        msg.id === messageId ? { ...msg, retryCount: msg.retryCount + 1 } : msg,
      ),
    }));
  }, []);

  const syncQueue = useCallback(async () => {
    if (state.isSyncing || !state.isOnline || state.queue.length === 0) {
      return;
    }

    setState((prev) => ({ ...prev, isSyncing: true }));

    // Process messages one at a time
    const message = state.queue[0];

    if (!message) {
      setState((prev) => ({ ...prev, isSyncing: false }));
      return;
    }

    try {
      // Dispatch custom event for the chat component to handle
      const event = new CustomEvent("copilot-queued-message", {
        detail: { id: message.id, content: message.content },
      });
      globalThis.dispatchEvent(event);

      // Wait for confirmation or timeout
      const confirmed = await new Promise<boolean>((resolve) => {
        const timeout = setTimeout(() => resolve(false), 10000);

        const handler = (e: Event) => {
          const customEvent = e as CustomEvent;
          if (customEvent.detail.id === message.id) {
            clearTimeout(timeout);
            globalThis.removeEventListener("copilot-message-sent", handler);
            resolve(customEvent.detail.success);
          }
        };

        globalThis.addEventListener("copilot-message-sent", handler);
      });

      if (confirmed) {
        removeFromQueue(message.id);
      } else if (message.retryCount < MAX_RETRIES) {
        incrementRetryCount(message.id);
        // Schedule next sync attempt
        syncTimeoutRef.current = setTimeout(
          () => {
            setState((prev) => ({ ...prev, isSyncing: false }));
            syncQueueRef.current?.();
          },
          RETRY_DELAY * (message.retryCount + 1),
        );
        return;
      } else {
        removeFromQueue(message.id);
      }
    } catch {
      console.error("Failed to sync message");
      if (message.retryCount < MAX_RETRIES) {
        incrementRetryCount(message.id);
      } else {
        removeFromQueue(message.id);
      }
    }

    setState((prev) => ({ ...prev, isSyncing: false }));

    // Continue with next message if queue not empty
    if (state.queue.length > 1) {
      syncTimeoutRef.current = setTimeout(() => syncQueueRef.current?.(), 500);
    }
  }, [
    state.isSyncing,
    state.isOnline,
    state.queue,
    removeFromQueue,
    incrementRetryCount,
  ]);

  // Keep ref updated
  useEffect(() => {
    syncQueueRef.current = syncQueue;
  }, [syncQueue]);

  // Load queue from localStorage on mount
  useEffect(() => {
    try {
      const stored = localStorage.getItem(QUEUE_STORAGE_KEY);
      if (stored) {
        const queue = JSON.parse(stored) as QueuedMessage[];
        setState((prev) => ({ ...prev, queue }));
      }
    } catch {
      console.error("Failed to load message queue");
    }
  }, []);

  // Save queue to localStorage whenever it changes
  useEffect(() => {
    try {
      localStorage.setItem(QUEUE_STORAGE_KEY, JSON.stringify(state.queue));
    } catch {
      console.error("Failed to save message queue");
    }
  }, [state.queue]);

  // Monitor online/offline status
  useEffect(() => {
    const handleOnline = () => {
      setState((prev) => ({ ...prev, isOnline: true }));
      // Trigger sync when coming back online
      if (state.queue.length > 0) {
        syncQueue();
      }
    };

    const handleOffline = () => {
      setState((prev) => ({ ...prev, isOnline: false }));
    };

    globalThis.addEventListener("online", handleOnline);
    globalThis.addEventListener("offline", handleOffline);

    return () => {
      globalThis.removeEventListener("online", handleOnline);
      globalThis.removeEventListener("offline", handleOffline);
      if (syncTimeoutRef.current) {
        clearTimeout(syncTimeoutRef.current);
      }
    };
  }, [state.queue.length, syncQueue]);

  const sendMessage = async (
    content: string,
    sendFn: (msg: string) => Promise<void>,
  ): Promise<{ success: boolean; queued: boolean }> => {
    if (!state.isOnline) {
      const message: QueuedMessage = {
        id: crypto.randomUUID(),
        content,
        timestamp: Date.now(),
        retryCount: 0,
      };
      setState((prev) => ({
        ...prev,
        queue: [...prev.queue, message],
      }));
      return { success: false, queued: true };
    }

    try {
      await sendFn(content);
      return { success: true, queued: false };
    } catch {
      // If send fails, add to queue for retry
      const message: QueuedMessage = {
        id: crypto.randomUUID(),
        content,
        timestamp: Date.now(),
        retryCount: 0,
      };
      setState((prev) => ({
        ...prev,
        queue: [...prev.queue, message],
      }));
      return { success: false, queued: true };
    }
  };

  const clearQueue = () => {
    setState((prev) => ({ ...prev, queue: [] }));
    localStorage.removeItem(QUEUE_STORAGE_KEY);
  };

  return {
    isOnline: state.isOnline,
    queue: state.queue,
    queueSize: state.queue.length,
    isSyncing: state.isSyncing,
    sendMessage,
    clearQueue,
    syncQueue,
  };
}
