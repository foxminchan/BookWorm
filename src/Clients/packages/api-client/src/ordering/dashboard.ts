import type { Dashboard } from "@workspace/types/ordering/dashboard";

import { apiClient } from "../client";
import axiosConfig from "../config";

const DASHBOARD_PATH = "/ordering/api/v1/dashboard";

/** Consume complete SSE frames, including frames split across network chunks. */
export async function readDashboardStream(
  body: ReadableStream<Uint8Array>,
  onSnapshot: (snapshot: Dashboard) => void,
  signal?: AbortSignal,
): Promise<void> {
  const reader = body.getReader();
  const abort = () => {
    void reader.cancel().catch(() => undefined);
  };
  signal?.addEventListener("abort", abort, { once: true });
  if (signal?.aborted) abort();
  const decoder = new TextDecoder();
  let buffer = "";
  try {
    for (;;) {
      const { done, value } = await reader.read();
      if (done) return;
      buffer += decoder.decode(value, { stream: true });
      let separator = /\r?\n\r?\n/.exec(buffer);
      while (separator?.index !== undefined) {
        const frame = buffer.slice(0, separator.index);
        buffer = buffer.slice(separator.index + separator[0].length);
        const lines = frame.split(/\r?\n/);
        const event = lines
          .find((line) => line.startsWith("event:"))
          ?.slice(6)
          .trim();
        const data = lines
          .filter((line) => line.startsWith("data:"))
          .map((line) => line.slice(5).replace(/^ /, ""))
          .join("\n");
        if (data && (!event || event === "dashboard")) {
          onSnapshot(JSON.parse(data) as Dashboard);
        }
        separator = /\r?\n\r?\n/.exec(buffer);
      }
    }
  } finally {
    signal?.removeEventListener("abort", abort);
    await reader.cancel().catch(() => undefined);
    reader.releaseLock();
  }
}

async function getDashboardStreamBody(
  response: Response,
): Promise<ReadableStream<Uint8Array>> {
  if (!response.ok || !response.body) {
    await response.body?.cancel();
    throw new Error("Dashboard stream unavailable");
  }
  if (!response.headers.get("content-type")?.includes("text/event-stream")) {
    await response.body.cancel();
    throw new Error("Invalid dashboard stream response");
  }
  return response.body;
}

function waitForRetry(delay: number, signal: AbortSignal): Promise<void> {
  return new Promise((resolve) => {
    if (signal.aborted) return resolve();
    const finish = () => {
      clearTimeout(timer);
      signal.removeEventListener("abort", finish);
      resolve();
    };
    const timer = setTimeout(finish, delay);
    signal.addEventListener("abort", finish, { once: true });
  });
}

const dashboardApiClient = {
  async get(signal?: AbortSignal): Promise<Dashboard> {
    const response = await apiClient.get<Dashboard>(DASHBOARD_PATH, { signal });
    return response.data;
  },

  subscribe(
    onSnapshot: (snapshot: Dashboard) => void,
    onDisconnect: () => void,
  ): () => void {
    const controller = new AbortController();
    const { signal } = controller;
    let delay = 1_000;
    const connect = async (): Promise<void> => {
      if (signal.aborted) return;
      try {
        const token = await apiClient.getAccessToken();
        if (signal.aborted) return;
        const response = await fetch(
          new URL(`${DASHBOARD_PATH}/stream`, axiosConfig.baseURL),
          {
            headers: {
              Accept: "text/event-stream",
              ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
            credentials: "include",
            cache: "no-store",
            signal,
          },
        );
        const body = await getDashboardStreamBody(response);
        await readDashboardStream(
          body,
          (snapshot) => {
            delay = 1_000;
            if (!signal.aborted) onSnapshot(snapshot);
          },
          signal,
        );
      } catch {
        if (signal.aborted) return;
      }
      if (signal.aborted) return;
      onDisconnect();
      return waitForRetry(delay, signal).then(() => {
        delay = Math.min(delay * 2, 30_000);
        return connect();
      });
    };
    void connect();
    return () => controller.abort();
  },
};

export default dashboardApiClient;
