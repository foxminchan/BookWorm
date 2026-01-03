import { renderHook } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useShopData } from "@/hooks/useShopData";

// Mock API hooks
vi.mock("@workspace/api-hooks/catalog/books/useBooks", () => ({
  default: vi.fn(() => ({
    data: { items: [], totalCount: 0 },
    isLoading: false,
  })),
}));

vi.mock("@workspace/api-hooks/catalog/categories/useCategories", () => ({
  default: vi.fn(() => ({
    data: [{ id: "1", name: "Category 1" }],
  })),
}));

vi.mock("@workspace/api-hooks/catalog/publishers/usePublishers", () => ({
  default: vi.fn(() => ({
    data: [{ id: "1", name: "Publisher 1" }],
  })),
}));

vi.mock("@workspace/api-hooks/catalog/authors/useAuthors", () => ({
  default: vi.fn(() => ({
    data: [{ id: "1", name: "Author 1" }],
  })),
}));

describe("useShopData", () => {
  const defaultParams = {
    currentPage: 1,
    priceRange: [0, 100],
    selectedCategories: [],
    selectedPublishers: [],
    selectedAuthors: [],
    searchQuery: "",
    sortBy: "name",
  };

  it("should return shop data", () => {
    const { result } = renderHook(() => useShopData(defaultParams));

    expect(result.current.booksData).toBeDefined();
    expect(result.current.categories).toBeDefined();
    expect(result.current.publishers).toBeDefined();
    expect(result.current.authors).toBeDefined();
    expect(result.current.isLoading).toBe(false);
  });

  it("should handle selected categories", () => {
    const params = {
      ...defaultParams,
      selectedCategories: ["1"],
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });

  it("should handle selected publishers", () => {
    const params = {
      ...defaultParams,
      selectedPublishers: ["1"],
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });

  it("should handle selected authors", () => {
    const params = {
      ...defaultParams,
      selectedAuthors: ["1"],
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });

  it("should handle search query", () => {
    const params = {
      ...defaultParams,
      searchQuery: "test query",
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });

  it("should handle price range", () => {
    const params = {
      ...defaultParams,
      priceRange: [10, 50],
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });

  it("should handle different sort options", () => {
    const sortOptions = ["name", "price-low", "price-high", "rating"];

    sortOptions.forEach((sortBy) => {
      const params = {
        ...defaultParams,
        sortBy,
      };

      const { result } = renderHook(() => useShopData(params));

      expect(result.current).toBeDefined();
    });
  });

  it("should handle pagination", () => {
    const params = {
      ...defaultParams,
      currentPage: 5,
    };

    const { result } = renderHook(() => useShopData(params));

    expect(result.current).toBeDefined();
  });
});
