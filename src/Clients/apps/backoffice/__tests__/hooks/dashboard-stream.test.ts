import { afterEach, describe, expect, it, vi } from "vitest";

import { apiClient } from "@workspace/api-client/client";
import dashboardApiClient, {
  readDashboardStream,
} from "@workspace/api-client/ordering/dashboard";
import type { Dashboard } from "@workspace/types/ordering/dashboard";

vi.unmock("@workspace/api-client/ordering/dashboard");

const snapshot: Dashboard = {
  totalOrders: 40,
  totalRevenue: 250,
  totalCustomers: 30,
  totalBooks: 35,
  dailyOrders: [],
  categories: [{ name: "Fictiön", value: 35 }],
  recentOrders: [],
  updatedAt: "2026-10-04T10:00:00Z",
};
const encoder = new TextEncoder();

afterEach(() => {
  vi.restoreAllMocks();
  vi.unstubAllGlobals();
  vi.useRealTimers();
});

describe("dashboard SSE transport", () => {
  it("decodes fragmented UTF-8 and CRLF frames and ignores heartbeat events", async () => {
    const text = `: heartbeat\r\n\r\nevent: heartbeat\r\ndata: ignored\r\n\r\nevent: dashboard\r\ndata: ${JSON.stringify(snapshot)}\r\n\r\n`;
    const bytes = encoder.encode(text);
    const body = new ReadableStream<Uint8Array>({
      start(controller) {
        for (const byte of bytes) controller.enqueue(new Uint8Array([byte]));
        controller.close();
      },
    });
    const receive = vi.fn();
    await readDashboardStream(body, receive);
    expect(receive).toHaveBeenCalledExactlyOnceWith(snapshot);
  });

  it("supports multiline data and multiple snapshots in one chunk", async () => {
    const updated = { ...snapshot, totalOrders: 41 };
    const multiline = JSON.stringify(snapshot, null, 2)
      .split("\n")
      .map((line) => `data: ${line}`)
      .join("\n");
    const body = new ReadableStream<Uint8Array>({
      start(controller) {
        controller.enqueue(
          encoder.encode(
            `${multiline}\n\nevent: dashboard\ndata: ${JSON.stringify(updated)}\n\n`,
          ),
        );
        controller.close();
      },
    });
    const receive = vi.fn();
    await readDashboardStream(body, receive);
    expect(receive.mock.calls).toEqual([[snapshot], [updated]]);
  });

  it("cancels a pending read when the dashboard unmounts", async () => {
    const cancelled = vi.fn();
    const controller = new AbortController();
    const body = new ReadableStream<Uint8Array>({ cancel: cancelled });
    const reading = readDashboardStream(body, vi.fn(), controller.signal);
    controller.abort();
    await reading;
    expect(cancelled).toHaveBeenCalledOnce();
  });

  it("reconnects using fresh bearer credentials and stops retries on unsubscribe", async () => {
    vi.useFakeTimers();
    vi.spyOn(apiClient, "getAccessToken")
      .mockResolvedValueOnce("first-token")
      .mockResolvedValue("fresh-token");
    const body = new ReadableStream<Uint8Array>({
      start(controller) {
        controller.enqueue(
          encoder.encode(
            `event: dashboard\ndata: ${JSON.stringify(snapshot)}\n\n`,
          ),
        );
      },
    });
    const fetchMock = vi
      .fn()
      .mockResolvedValueOnce(new Response(null, { status: 503 }))
      .mockResolvedValue(
        new Response(body, {
          headers: { "Content-Type": "text/event-stream" },
        }),
      );
    vi.stubGlobal("fetch", fetchMock);
    const receive = vi.fn();
    const disconnect = vi.fn();
    const unsubscribe = dashboardApiClient.subscribe(receive, disconnect);
    await vi.advanceTimersByTimeAsync(0);
    expect(disconnect).toHaveBeenCalledOnce();
    await vi.advanceTimersByTimeAsync(1_000);
    expect(fetchMock).toHaveBeenCalledTimes(2);
    expect(fetchMock.mock.calls[0]![1].headers.Authorization).toBe(
      "Bearer first-token",
    );
    expect(fetchMock.mock.calls[1]![1].headers.Authorization).toBe(
      "Bearer fresh-token",
    );
    expect(receive).toHaveBeenCalledExactlyOnceWith(snapshot);
    unsubscribe();
    await vi.advanceTimersByTimeAsync(60_000);
    expect(fetchMock).toHaveBeenCalledTimes(2);
  });
});
