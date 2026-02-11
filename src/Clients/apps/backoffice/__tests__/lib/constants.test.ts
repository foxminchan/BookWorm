import { describe, expect, it } from "vitest";

import {
  API,
  AUTH,
  CHART_COLORS,
  CHART_THEME,
  DEFAULT_BOOKS_PAGE_SIZE,
  DEFAULT_PAGE_SIZE,
  PAGE_SIZES,
  currencyFormatter,
} from "@/lib/constants";

describe("AUTH constants", () => {
  it("should have keycloak provider", () => {
    expect(AUTH.PROVIDER).toBe("keycloak");
  });

  it("should have root callback URL", () => {
    expect(AUTH.CALLBACK_URL).toBe("/");
  });
});

describe("API constants", () => {
  it("should have default retry count of 3", () => {
    expect(API.DEFAULT_RETRY).toBe(3);
  });

  it("should have default timeout of 30000ms", () => {
    expect(API.DEFAULT_TIMEOUT).toBe(30000);
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
