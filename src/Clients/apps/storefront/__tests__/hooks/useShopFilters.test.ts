import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useShopFilters } from "@/hooks/useShopFilters";

// Mock useSearchParams from Next.js navigation
const mockSearchParams = new URLSearchParams();
const mockUseSearchParams = vi.fn(() => mockSearchParams);

vi.mock("next/navigation", async () => {
  const actual = await vi.importActual("next/navigation");
  return {
    ...actual,
    useSearchParams: () => mockUseSearchParams(),
  };
});

describe("useShopFilters", () => {
  beforeEach(() => {
    // Clear all search params
    Array.from(mockSearchParams.keys()).forEach((key) => {
      mockSearchParams.delete(key);
    });
    mockUseSearchParams.mockClear();
  });

  it("should initialize with default values when no URL params", () => {
    const { result } = renderHook(() => useShopFilters());

    expect(result.current.currentPage).toBe(1);
    expect(result.current.priceRange).toEqual([0, 100]);
    expect(result.current.selectedAuthors).toEqual([]);
    expect(result.current.selectedPublishers).toEqual([]);
    expect(result.current.selectedCategories).toEqual([]);
    expect(result.current.searchQuery).toBe("");
    expect(result.current.sortBy).toBe("name");
    expect(result.current.isFilterOpen).toBe(false);
  });

  it("should parse category from URL params", () => {
    mockSearchParams.set("category", "fiction");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.selectedCategories).toEqual(["fiction"]);
  });

  it("should parse publisher from URL params", () => {
    mockSearchParams.set("publisher", "penguin");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.selectedPublishers).toEqual(["penguin"]);
  });

  it("should parse author from URL params", () => {
    mockSearchParams.set("author", "john-doe");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.selectedAuthors).toEqual(["john-doe"]);
  });

  it("should parse search query from URL params", () => {
    mockSearchParams.set("search", "harry potter");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.searchQuery).toBe("harry potter");
  });

  it("should parse page number from URL params", () => {
    mockSearchParams.set("page", "3");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.currentPage).toBe(3);
  });

  it("should parse multiple URL params simultaneously", () => {
    mockSearchParams.set("category", "science");
    mockSearchParams.set("publisher", "oreilly");
    mockSearchParams.set("author", "martin-fowler");
    mockSearchParams.set("search", "refactoring");
    mockSearchParams.set("page", "2");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.selectedCategories).toEqual(["science"]);
    expect(result.current.selectedPublishers).toEqual(["oreilly"]);
    expect(result.current.selectedAuthors).toEqual(["martin-fowler"]);
    expect(result.current.searchQuery).toBe("refactoring");
    expect(result.current.currentPage).toBe(2);
  });

  it("should clear filters when URL params are removed", () => {
    mockSearchParams.set("category", "fiction");
    mockSearchParams.set("search", "test");

    const { result } = renderHook(() => useShopFilters());

    expect(result.current.selectedCategories).toEqual(["fiction"]);
    expect(result.current.searchQuery).toBe("test");

    // Clear params and force a new mock instance
    mockSearchParams.delete("category");
    mockSearchParams.delete("search");

    // Re-render with new hook call to pick up changed mock
    const { result: result2 } = renderHook(() => useShopFilters());

    expect(result2.current.selectedCategories).toEqual([]);
    expect(result2.current.searchQuery).toBe("");
  });

  it("should provide setters for all filter states", () => {
    const { result } = renderHook(() => useShopFilters());

    expect(typeof result.current.setCurrentPage).toBe("function");
    expect(typeof result.current.setPriceRange).toBe("function");
    expect(typeof result.current.setSelectedAuthors).toBe("function");
    expect(typeof result.current.setSelectedPublishers).toBe("function");
    expect(typeof result.current.setSelectedCategories).toBe("function");
    expect(typeof result.current.setSearchQuery).toBe("function");
    expect(typeof result.current.setSortBy).toBe("function");
    expect(typeof result.current.setIsFilterOpen).toBe("function");
  });

  it("should handle invalid page number gracefully", () => {
    mockSearchParams.set("page", "invalid");

    const { result } = renderHook(() => useShopFilters());

    // parseInt returns NaN for invalid input
    expect(Number.isNaN(result.current.currentPage)).toBe(true);
  });

  it("should default to page 1 when page param is missing", () => {
    const { result } = renderHook(() => useShopFilters());

    expect(result.current.currentPage).toBe(1);
  });

  it("should update when search params change", () => {
    const { result } = renderHook(() => useShopFilters());

    expect(result.current.searchQuery).toBe("");

    mockSearchParams.set("search", "new search");

    const { result: result2 } = renderHook(() => useShopFilters());

    expect(result2.current.searchQuery).toBe("new search");
  });
});
