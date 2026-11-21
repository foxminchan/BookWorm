import { type PropsWithChildren, createElement } from "react";

import { QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import authorsApiClient from "@workspace/api-client/catalog/authors";
import useCreateAuthor from "@workspace/api-hooks/catalog/authors/useCreateAuthor";
import { catalogKeys } from "@workspace/api-hooks/keys";
import { getQueryClient } from "@workspace/api-hooks/query-client";

import { createTestQueryClient } from "@/__tests__/utils/test-utils";

describe("api-hooks TanStack Query enhancements", () => {
  it("preserves cache invalidation when caller passes onSuccess to mutation hook", async () => {
    const client = createTestQueryClient();
    const invalidateSpy = vi.spyOn(client, "invalidateQueries");
    const createdAuthor = { id: "author-1", name: "Ada Lovelace" };
    vi.spyOn(authorsApiClient, "create").mockResolvedValueOnce(createdAuthor);

    const callerOnSuccess = vi.fn();

    const wrapper = ({ children }: PropsWithChildren) =>
      createElement(QueryClientProvider, { client }, children);

    const { result } = renderHook(
      () => useCreateAuthor({ onSuccess: callerOnSuccess }),
      { wrapper },
    );

    act(() => {
      result.current.mutate({ name: "Ada Lovelace" });
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(invalidateSpy).toHaveBeenCalledWith({
      queryKey: catalogKeys.authors.lists(),
    });
    expect(callerOnSuccess).toHaveBeenCalledWith(
      createdAuthor,
      { name: "Ada Lovelace" },
      undefined,
      expect.objectContaining({ client }),
    );
  });

  it("configures QueryClient retry to skip permanent 4xx errors and cap transient retries", () => {
    const client = getQueryClient();
    const retry = client.getDefaultOptions().queries?.retry;

    expect(typeof retry).toBe("function");
    if (typeof retry !== "function") return;

    const withStatus = (status: number) =>
      Object.assign(new Error(`HTTP ${status}`), { status });
    const withAxiosStatus = (status: number) =>
      Object.assign(new Error(`HTTP ${status}`), { response: { status } });

    expect(retry(0, withStatus(404))).toBe(false);
    expect(retry(0, withStatus(400))).toBe(false);
    expect(retry(0, withAxiosStatus(412))).toBe(false);

    expect(retry(0, withStatus(408))).toBe(true);
    expect(retry(0, withStatus(429))).toBe(true);
    expect(retry(0, withAxiosStatus(503))).toBe(true);
    expect(retry(1, new Error("Network Error"))).toBe(true);
    expect(retry(2, new Error("Network Error"))).toBe(false);
  });
});
