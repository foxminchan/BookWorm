import type { SortingState } from "@tanstack/react-table";
import { act, renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { usePaginatedTable } from "@/hooks/usePaginatedTable";

describe("usePaginatedTable", () => {
  it("should initialize with default page size of 10", () => {
    const { result } = renderHook(() => usePaginatedTable());

    expect(result.current.pageIndex).toBe(0);
    expect(result.current.pageSize).toBe(10);
    expect(result.current.sorting).toEqual([]);
    expect(result.current.apiPageIndex).toBe(1);
  });

  it("should initialize with custom page size", () => {
    const { result } = renderHook(() =>
      usePaginatedTable({ initialPageSize: 25 }),
    );

    expect(result.current.pageSize).toBe(25);
    expect(result.current.pageIndex).toBe(0);
  });

  it("should update pagination via handlePaginationChange", () => {
    const { result } = renderHook(() => usePaginatedTable());

    act(() => {
      result.current.handlePaginationChange(2, 20);
    });

    expect(result.current.pageIndex).toBe(2);
    expect(result.current.pageSize).toBe(20);
    expect(result.current.apiPageIndex).toBe(3);
  });

  it("should update sorting via handleSortingChange", () => {
    const { result } = renderHook(() => usePaginatedTable());

    const newSorting: SortingState = [{ id: "name", desc: false }];

    act(() => {
      result.current.handleSortingChange(newSorting);
    });

    expect(result.current.sorting).toEqual(newSorting);
  });

  it("should compute sortingQuery from sorting state", () => {
    const { result } = renderHook(() => usePaginatedTable());

    act(() => {
      result.current.handleSortingChange([{ id: "price", desc: true }]);
    });

    expect(result.current.sortingQuery).toEqual({
      orderBy: "price",
      isDescending: true,
    });
  });

  it("should return empty sortingQuery when no sorting applied", () => {
    const { result } = renderHook(() => usePaginatedTable());

    expect(result.current.sortingQuery).toEqual({});
  });

  it("should return empty sortingQuery after clearing sorting", () => {
    const { result } = renderHook(() => usePaginatedTable());

    act(() => {
      result.current.handleSortingChange([{ id: "name", desc: false }]);
    });

    expect(result.current.sortingQuery).toEqual({
      orderBy: "name",
      isDescending: false,
    });

    act(() => {
      result.current.handleSortingChange([]);
    });

    expect(result.current.sortingQuery).toEqual({});
  });

  it("should compute apiPageIndex as 1-based offset", () => {
    const { result } = renderHook(() => usePaginatedTable());

    expect(result.current.apiPageIndex).toBe(1);

    act(() => {
      result.current.handlePaginationChange(4, 10);
    });

    expect(result.current.apiPageIndex).toBe(5);
  });
});
