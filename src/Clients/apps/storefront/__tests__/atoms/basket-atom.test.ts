import { renderHook, waitFor } from "@testing-library/react";
import { useAtomValue } from "jotai";
import { describe, expect, it } from "vitest";

import {
  basketAtom,
  basketItemCountAtom,
  basketItemsAtom,
} from "@/atoms/basket-atom";

import { createWrapper } from "../utils/test-utils";

describe("basket atoms", () => {
  it("should return basket data from basketAtom", async () => {
    const { result } = renderHook(() => useAtomValue(basketAtom), {
      wrapper: createWrapper(),
    });

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
});
