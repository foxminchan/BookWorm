import { describe, expect, it } from "vitest";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import { currencyFormatter } from "@workspace/utils/constants";

import {
  CHART_COLORS,
  CHART_THEME,
  DEFAULT_BOOKS_PAGE_SIZE,
  DEFAULT_PAGE_SIZE,
  PAGE_SIZES,
} from "@/lib/constants";

describe("API_ENDPOINTS", () => {
  it("exposes default parameterized routes for MSW handlers", () => {
    expect(API_ENDPOINTS.catalog.books.root).toBe("/catalog/api/v1/books");
    expect(API_ENDPOINTS.catalog.books.byId()).toBe(
      "/catalog/api/v1/books/:id",
    );
    expect(API_ENDPOINTS.ordering.orders.cancel()).toBe(
      "/ordering/api/v1/orders/:id/cancel",
    );
    expect(API_ENDPOINTS.ordering.orders.complete()).toBe(
      "/ordering/api/v1/orders/:id/complete",
    );
    expect(API_ENDPOINTS.rating.feedbacks.summarize()).toBe(
      "/rating/api/v1/feedbacks/:bookId/summarize",
    );
  });

  it("builds concrete resource paths when an ID is supplied", () => {
    expect(API_ENDPOINTS.catalog.books.byId("book-123")).toBe(
      "/catalog/api/v1/books/book-123",
    );
    expect(API_ENDPOINTS.ordering.orders.history("order-456")).toBe(
      "/ordering/api/v1/orders/order-456/history",
    );
    expect(API_ENDPOINTS.ordering.orders.summary("order-456")).toBe(
      "/ordering/api/v1/orders/order-456/summary",
    );
    expect(API_ENDPOINTS.rating.feedbacks.summarize("book-789")).toBe(
      "/rating/api/v1/feedbacks/book-789/summarize",
    );
  });
});

describe("Pagination constants", () => {
  it("should have valid page sizes", () => {
    expect(PAGE_SIZES).toEqual([10, 20, 50]);
  });

  it("should have default page size of 10", () => {
    expect(DEFAULT_PAGE_SIZE).toBe(10);
  });

  it("should have default books page size of 100", () => {
    expect(DEFAULT_BOOKS_PAGE_SIZE).toBe(100);
  });
});

describe("CHART_COLORS", () => {
  it("should have 5 color values", () => {
    expect(CHART_COLORS).toHaveLength(5);
  });

  it("should contain valid hex color strings", () => {
    for (const color of CHART_COLORS) {
      expect(color).toMatch(/^#[0-9a-f]{6}$/);
    }
  });
});

describe("currencyFormatter", () => {
  it("should format integer amounts as USD currency", () => {
    expect(currencyFormatter.format(100)).toBe("$100.00");
  });

  it("should format decimal amounts correctly", () => {
    expect(currencyFormatter.format(29.99)).toBe("$29.99");
  });

  it("should format zero", () => {
    expect(currencyFormatter.format(0)).toBe("$0.00");
  });

  it("should format large amounts with comma separators", () => {
    expect(currencyFormatter.format(1234567.89)).toBe("$1,234,567.89");
  });

  it("should format negative amounts", () => {
    const formatted = currencyFormatter.format(-42.5);
    expect(formatted).toContain("42.50");
    expect(formatted).toContain("-");
  });
});

describe("CHART_THEME", () => {
  it("should have tooltip settings", () => {
    expect(CHART_THEME.tooltip.backgroundColor).toBe("#171717");
    expect(CHART_THEME.tooltip.border).toBe("1px solid #262626");
  });

  it("should have grid settings", () => {
    expect(CHART_THEME.grid.stroke).toBe("#262626");
    expect(CHART_THEME.grid.strokeDasharray).toBe("3 3");
  });

  it("should have axis settings", () => {
    expect(CHART_THEME.axis.stroke).toBe("#a1a1a1");
  });
});
