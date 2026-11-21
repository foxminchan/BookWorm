import { useQueryClient } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react";
import { useAtomValue, useSetAtom } from "jotai";
import { describe, expect, it } from "vitest";

import { basketKeys } from "@workspace/api-hooks/keys";

import {
  basketAtom,
  basketItemCountAtom,
  basketItemsAtom,
  isAuthenticatedAtom,
} from "@/atoms/basket-atom";

import { createWrapper } from "../utils/test-utils";

describe("basket atoms", () => {
  it("should return basket data from basketAtom", async () => {
    const { result } = renderHook(
      () => {
        const setAuth = useSetAtom(isAuthenticatedAtom);
        setAuth(true);
        return useAtomValue(basketAtom);
      },
      {
        wrapper: createWrapper(),
      },
    );

    await waitFor(() => {
      expect(result.current.data).toBeDefined();
    });
  });

  it("should calculate total item count from basketItemCountAtom", async () => {
    const { result } = renderHook(() => useAtomValue(basketItemCountAtom), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(typeof result.current).toBe("number");
    });
  });

  it("should return empty array when no basket items", async () => {
    const { result } = renderHook(() => useAtomValue(basketItemsAtom), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(Array.isArray(result.current)).toBe(true);
    });
  });

  it("should return basket items array from basketItemsAtom", async () => {
    const { result } = renderHook(() => useAtomValue(basketItemsAtom), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(Array.isArray(result.current)).toBe(true);
    });
  });

  it("should return a stable empty array reference when basket has no items", () => {
    const { result, rerender } = renderHook(
      () => useAtomValue(basketItemsAtom),
      {
        wrapper: createWrapper(),
      },
    );

    const firstResult = result.current;
    rerender();
    expect(result.current).toBe(firstResult);
    expect(result.current).toEqual([]);
  });

  it("should share QueryClient cache with jotai-tanstack-query via queryClientAtom", async () => {
    const { result } = renderHook(
      () => ({
        queryClient: useQueryClient(),
        items: useAtomValue(basketItemsAtom),
        itemCount: useAtomValue(basketItemCountAtom),
      }),
      {
        wrapper: createWrapper(),
      },
    );

    act(() => {
      result.current.queryClient.setQueryData(basketKeys.detail(), {
        id: "basket-1",
        items: [
          {
            id: "book-1",
            name: "Clean Code",
            quantity: 2,
            price: 30,
          },
        ],
      });
    });

    await waitFor(() => {
      expect(result.current.items).toHaveLength(1);
      expect(result.current.itemCount).toBe(2);
    });
  });
});
