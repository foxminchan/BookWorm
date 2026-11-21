import { type PropsWithChildren, createElement } from "react";

import { QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it } from "vitest";

import { isPreconditionFailedError } from "@workspace/api-client/client";
import useBook from "@workspace/api-hooks/catalog/books/useBook";
import useUpdateBook from "@workspace/api-hooks/catalog/books/useUpdateBook";
import useOrderSummary from "@workspace/api-hooks/ordering/orders/useOrderSummary";
import { booksStore, mockBooks } from "@workspace/mocks/catalog/books";
import { server } from "@workspace/mocks/node";
import { ordersStoreManager } from "@workspace/mocks/ordering/orders";

import { createTestQueryClient } from "@/__tests__/utils/test-utils";

type RecordedExchange = {
  method: string;
  url: string;
  ifNoneMatch: string | null;
  ifMatch: string | null;
  status: number;
  etag: string | null;
};

function createWrapper() {
  const client = createTestQueryClient();
  return function ETagTestProvider({ children }: PropsWithChildren) {
    return createElement(QueryClientProvider, { client }, children);
  };
}

describe("ETag and conditional request hooks", () => {
  let exchanges: RecordedExchange[] = [];

  const handleMockedResponse = ({
    request,
    response,
  }: {
    request: Request;
    response: Response;
  }) => {
    if (request.method === "OPTIONS") {
      return;
    }

    exchanges.push({
      method: request.method,
      url: request.url,
      ifNoneMatch: request.headers.get("If-None-Match"),
      ifMatch: request.headers.get("If-Match"),
      status: response.status,
      etag: response.headers.get("ETag"),
    });
  };

  beforeEach(() => {
    exchanges = [];
    server.events.on("response:mocked", handleMockedResponse);
  });

  afterEach(() => {
    server.events.removeListener("response:mocked", handleMockedResponse);
    booksStore.reset();
    ordersStoreManager.reset();
  });

  it("caches ETag on useBook, sends If-None-Match on refetch, and handles 304 and 200 updates", async () => {
    const targetBook = mockBooks[0]!;
    const wrapper = createWrapper();

    const { result } = renderHook(() => useBook(targetBook.id), { wrapper });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.id).toBe(targetBook.id);
    expect(result.current.data?.version).toBe(1);
    expect(result.current.etag).toBe('"1"');
    expect(exchanges[0]).toMatchObject({
      method: "GET",
      ifNoneMatch: null,
      status: 200,
      etag: '"1"',
    });

    await act(async () => {
      await result.current.refetch();
    });

    expect(exchanges[1]).toMatchObject({
      method: "GET",
      ifNoneMatch: '"1"',
      status: 304,
      etag: '"1"',
    });
    expect(result.current.data?.name).toBe(targetBook.name);
    expect(result.current.etag).toBe('"1"');

    // Simulate concurrent server-side modification bumping version to 2
    booksStore.update({
      id: targetBook.id,
      name: "Updated Title On Server",
      description: targetBook.description ?? "Updated description",
      price: targetBook.price,
      priceSale: targetBook.priceSale,
      categoryId: targetBook.category!.id,
      publisherId: targetBook.publisher!.id,
      authorIds: targetBook.authors.map((a) => a.id),
    });

    await act(async () => {
      await result.current.refetch();
    });

    expect(exchanges[2]).toMatchObject({
      method: "GET",
      ifNoneMatch: '"1"',
      status: 200,
      etag: '"2"',
    });
    expect(result.current.data?.name).toBe("Updated Title On Server");
    expect(result.current.data?.version).toBe(2);
    expect(result.current.etag).toBe('"2"');
  });

  it("sends If-Match via useUpdateBook, invalidates detail query, and surfaces 412 conflicts", async () => {
    const targetBook = mockBooks.find(
      (b) => b.id === "c1c2d3e4-f5a6-7b8c-9d0e-1f2a3b4c5d6f",
    )!;
    const wrapper = createWrapper();

    const { result } = renderHook(
      () => ({
        book: useBook(targetBook.id),
        update: useUpdateBook(),
      }),
      { wrapper },
    );

    await waitFor(() => expect(result.current.book.isSuccess).toBe(true));
    expect(result.current.book.etag).toBe('"1"');

    const updatePayload = {
      id: targetBook.id,
      name: "Edited Book Name",
      description: targetBook.description ?? "Edited description",
      price: targetBook.price,
      priceSale: targetBook.priceSale,
      categoryId: targetBook.category!.id,
      publisherId: targetBook.publisher!.id,
      authorIds: targetBook.authors.map((a) => a.id),
    };

    await act(async () => {
      await result.current.update.mutateAsync({
        request: updatePayload,
        ifMatch: result.current.book.etag,
      });
    });

    const putExchange = exchanges.find((e) => e.method === "PUT");
    expect(putExchange).toMatchObject({
      method: "PUT",
      ifMatch: '"1"',
      status: 204,
    });

    await waitFor(() => expect(result.current.book.data?.version).toBe(2));
    expect(result.current.book.data?.name).toBe("Edited Book Name");
    expect(result.current.book.etag).toBe('"2"');

    // Sending stale If-Match "1" when server is at version 2 triggers 412 Precondition Failed
    let caughtError: unknown;
    await act(async () => {
      try {
        await result.current.update.mutateAsync({
          request: {
            ...updatePayload,
            name: "Stale Edit",
          },
          ifMatch: '"1"',
        });
      } catch (error) {
        caughtError = error;
      }
    });

    expect(isPreconditionFailedError(caughtError)).toBe(true);
    const failedPut = exchanges.filter((e) => e.method === "PUT").at(-1);
    expect(failedPut).toMatchObject({
      method: "PUT",
      ifMatch: '"1"',
      status: 412,
    });
  });

  it("caches ETag and honors 304 Not Modified on useOrderSummary including versioned replays", async () => {
    const order = ordersStoreManager
      .list()
      .find((o) => o.status === "Completed")!;
    const wrapper = createWrapper();

    const { result } = renderHook(() => useOrderSummary(order.id), {
      wrapper,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(result.current.data?.version).toBe(2);
    expect(result.current.etag).toBe('"2"');
    expect(exchanges[0]).toMatchObject({
      method: "GET",
      ifNoneMatch: null,
      status: 200,
      etag: '"2"',
    });

    await act(async () => {
      await result.current.refetch();
    });

    expect(exchanges[1]).toMatchObject({
      method: "GET",
      ifNoneMatch: '"2"',
      status: 304,
      etag: '"2"',
    });
    expect(result.current.data?.status).toBe("Completed");
    expect(result.current.etag).toBe('"2"');

    const { result: replayResult } = renderHook(
      () => useOrderSummary(order.id, { version: 1 }),
      { wrapper },
    );

    await waitFor(() => expect(replayResult.current.isSuccess).toBe(true));

    expect(replayResult.current.data?.version).toBe(1);
    expect(replayResult.current.data?.status).toBe("New");
    expect(replayResult.current.etag).toBe('"1"');
    expect(exchanges[2]).toMatchObject({
      method: "GET",
      ifNoneMatch: null,
      status: 200,
      etag: '"1"',
    });
  });
});
