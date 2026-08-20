import { describe, expect, it } from "vitest";

import {
  getOrderStatusColor,
  getOrderStatusColorBordered,
  getReviewSortParams,
  getShopSortParams,
} from "@/lib/pattern";

describe("pattern utils", () => {
  describe("getShopSortParams", () => {
    it.each([
      ["price-low", "price-low", { orderBy: "price", isDescending: false }],
      ["price-high", "price-high", { orderBy: "price", isDescending: true }],
      ["rating", "rating", { orderBy: "averageRating", isDescending: true }],
      ["name", "name", { orderBy: "name", isDescending: false }],
      ["unknown value", "invalid", { orderBy: "name", isDescending: false }],
    ])("should return correct params for %s", (_label, value, expected) => {
      const result = getShopSortParams(value as any);
      expect(result).toEqual(expected);
    });
  });

  describe("getReviewSortParams", () => {
    it.each([
      ["newest", "newest", { orderBy: "createdAt", isDescending: true }],
      ["highest", "highest", { orderBy: "rating", isDescending: true }],
      ["lowest", "lowest", { orderBy: "rating", isDescending: false }],
    ])("should return correct params for %s", (_label, value, expected) => {
      const result = getReviewSortParams(value as any);
      expect(result).toEqual(expected);
    });
  });

  describe("getOrderStatusColor", () => {
    it.each([
      ["Completed", "Completed", "green"],
      ["Cancelled", "Cancelled", "red"],
      ["New", "New", "blue"],
      ["unknown status", "Unknown" as any, "gray"],
    ])("should return %s classes for %s", (_label, status, expectedColor) => {
      const result = getOrderStatusColor(status);
      expect(result).toContain(expectedColor);
    });

    it("should include dark mode classes", () => {
      const result = getOrderStatusColor("Completed");
      expect(result).toContain("dark:");
    });
  });

  describe("getOrderStatusColorBordered", () => {
    it.each([
      ["Completed", "Completed", "green"],
      ["Cancelled", "Cancelled", "red"],
      ["New", "New", "blue"],
    ])(
      "should return %s bordered classes for %s",
      (_label, status, expectedColor) => {
        const result = getOrderStatusColorBordered(status as any);
        expect(result).toContain(expectedColor);
        expect(result).toContain("border");
      },
    );

    it("should return gray classes for unknown status", () => {
      const result = getOrderStatusColorBordered("Unknown" as any);
      expect(result).toContain("gray");
      expect(result).not.toContain("border");
    });

    it("should include dark mode classes", () => {
      const result = getOrderStatusColorBordered("Completed" as any);
      expect(result).toContain("dark:");
    });
  });
});
