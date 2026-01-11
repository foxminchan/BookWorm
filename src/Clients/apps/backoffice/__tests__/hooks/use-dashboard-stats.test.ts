import { renderHook } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { createWrapper } from "@/__tests__/utils/test-utils";
import { useDashboardStats } from "@/hooks/use-dashboard-stats";

describe("useDashboardStats", () => {
  it("should initialize with loading state", () => {
    const { result } = renderHook(() => useDashboardStats(), {
      wrapper: createWrapper(),
    });

    // Initially should be loading
    expect(result.current.isLoading).toBe(true);
    expect(result.current.books).toEqual([]);
    expect(result.current.orders).toEqual([]);
    expect(result.current.customers).toEqual([]);
  });
});
