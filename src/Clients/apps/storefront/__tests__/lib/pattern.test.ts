import { describe, expect, it } from "vitest";

import {
  getOrderStatusColor,
  getOrderStatusColorBordered,
  getReviewSortParams,
  getShopSortParams,
} from "@/lib/pattern";

describe("pattern utils", () => {
  describe("getShopSortParams", () => {
    it("should return correct params for price-low", () => {
      const result = getShopSortParams("price-low");
      expect(result).toEqual({ orderBy: "price", isDescending: false });
    });

    it("should return correct params for price-high", () => {
      const result = getShopSortParams("price-high");
      expect(result).toEqual({ orderBy: "price", isDescending: true });
    });

    it("should return correct params for rating", () => {
      const result = getShopSortParams("rating");
      expect(result).toEqual({ orderBy: "averageRating", isDescending: true });
    });

    it("should return correct params for name", () => {
      const result = getShopSortParams("name");
      expect(result).toEqual({ orderBy: "name", isDescending: false });
    });

    it("should return default params for unknown value", () => {
      const result = getShopSortParams("invalid");
      expect(result).toEqual({ orderBy: "name", isDescending: false });
    });
  });

  describe("getReviewSortParams", () => {
    it("should return correct params for newest", () => {
      const result = getReviewSortParams("newest");
      expect(result).toEqual({ orderBy: "createdAt", isDescending: true });
    });

    it("should return correct params for highest", () => {
      const result = getReviewSortParams("highest");
      expect(result).toEqual({ orderBy: "rating", isDescending: true });
    });

    it("should return correct params for lowest", () => {
      const result = getReviewSortParams("lowest");
      expect(result).toEqual({ orderBy: "rating", isDescending: false });
    });
  });

  describe("getOrderStatusColor", () => {
    it("should return green classes for Completed", () => {
      const result = getOrderStatusColor("Completed");
      expect(result).toContain("green");
    });

    it("should return red classes for Cancelled", () => {
      const result = getOrderStatusColor("Cancelled");
      expect(result).toContain("red");
    });

    it("should return blue classes for New", () => {
      const result = getOrderStatusColor("New");
      expect(result).toContain("blue");
    });

    it("should return gray classes for unknown status", () => {
      const result = getOrderStatusColor("Unknown" as any);
      expect(result).toContain("gray");
    });

    it("should include dark mode classes", () => {
      const result = getOrderStatusColor("Completed");
      expect(result).toContain("dark:");
    });
  });

  describe("getOrderStatusColorBordered", () => {
    it("should return green bordered classes for Completed", () => {
      const result = getOrderStatusColorBordered("Completed");
      expect(result).toContain("green");
      expect(result).toContain("border");
    });

    it("should return red bordered classes for Cancelled", () => {
      const result = getOrderStatusColorBordered("Cancelled");
      expect(result).toContain("red");
      expect(result).toContain("border");
    });

    it("should return blue bordered classes for New", () => {
      const result = getOrderStatusColorBordered("New");
      expect(result).toContain("blue");
      expect(result).toContain("border");
    });

    it("should return gray classes for unknown status", () => {
      const result = getOrderStatusColorBordered("Unknown" as any);
      expect(result).toContain("gray");
    });

    it("should include dark mode classes", () => {
      const result = getOrderStatusColorBordered("Completed");
      expect(result).toContain("dark:");
    });
  });
});
