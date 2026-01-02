import { act, renderHook, waitFor } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { useDebounceValue } from "@/hooks/useDebounceValue";

describe("useDebounceValue", () => {
  it("should return initial value immediately", () => {
    const { result } = renderHook(() => useDebounceValue("initial", 500));

    expect(result.current).toBe("initial");
  });

  it("should debounce value changes", async () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounceValue(value, delay),
      {
        initialProps: { value: "first", delay: 300 },
      },
    );

    expect(result.current).toBe("first");

    // Change value
    rerender({ value: "second", delay: 300 });

    // Should still be 'first' immediately after change
    expect(result.current).toBe("first");

    // Wait for debounce
    await waitFor(
      () => {
        expect(result.current).toBe("second");
      },
      { timeout: 500 },
    );
  });

  it("should reset timer on rapid value changes", async () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounceValue(value, delay),
      {
        initialProps: { value: "initial", delay: 300 },
      },
    );

    // Rapid changes
    rerender({ value: "change1", delay: 300 });
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 100));
    });

    rerender({ value: "change2", delay: 300 });
    await act(async () => {
      await new Promise((resolve) => setTimeout(resolve, 100));
    });

    rerender({ value: "final", delay: 300 });

    // Should still have initial value
    expect(result.current).toBe("initial");

    // Wait for final debounce
    await waitFor(
      () => {
        expect(result.current).toBe("final");
      },
      { timeout: 500 },
    );
  });

  it("should handle different delay times", async () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounceValue(value, delay),
      {
        initialProps: { value: "test", delay: 100 },
      },
    );

    rerender({ value: "changed", delay: 100 });

    await waitFor(
      () => {
        expect(result.current).toBe("changed");
      },
      { timeout: 200 },
    );
  });

  it("should handle complex objects", async () => {
    const initialObj = { name: "John", age: 30 };
    const updatedObj = { name: "Jane", age: 25 };

    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounceValue(value, delay),
      {
        initialProps: { value: initialObj, delay: 200 },
      },
    );

    expect(result.current).toBe(initialObj);

    rerender({ value: updatedObj, delay: 200 });

    await waitFor(
      () => {
        expect(result.current).toBe(updatedObj);
      },
      { timeout: 300 },
    );
  });

  it("should handle number values", async () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounceValue(value, delay),
      {
        initialProps: { value: 0, delay: 200 },
      },
    );

    expect(result.current).toBe(0);

    rerender({ value: 100, delay: 200 });

    await waitFor(
      () => {
        expect(result.current).toBe(100);
      },
      { timeout: 300 },
    );
  });
});
