import { describe, expect, it } from "vitest";

import {
  calculateDiscount,
  formatCompactDate,
  formatDate,
  formatPrice,
  truncateText,
} from "@workspace/utils/format";

describe("format utils", () => {
  describe("formatPrice", () => {
    it.each([
      ["price with dollar sign", 19.99, "$19.99"],
      ["whole numbers", 50, "$50.00"],
      ["large numbers with commas", 1234.56, "$1,234.56"],
      ["zero", 0, "$0.00"],
      ["negative numbers", -15.5, "-$15.50"],
    ])("should %s", (_label, value, expected) => {
      expect(formatPrice(value)).toBe(expected);
    });
  });

  describe("formatDate", () => {
    it.each([
      ["date string", "2024-01-15", "January 15, 2024"],
      ["Date object", new Date("2024-12-25"), "December 25, 2024"],
    ])("should %s", (_label, value, expected) => {
      const result = formatDate(value as string | Date);
      expect(result).toBe(expected);
    });

    it("should handle ISO date strings", () => {
      const result = formatDate("2024-03-10T10:30:00Z");
      expect(result).toContain("March");
      expect(result).toContain("2024");
    });
  });

  describe("formatCompactDate", () => {
    it.each([
      ["date in compact style", "2024-01-15", "Jan 15, 2024"],
      ["Date object compactly", new Date("2024-12-25"), "Dec 25, 2024"],
      ["abbreviated month names", "2024-09-01", "Sep 1, 2024"],
    ])("should %s", (_label, value, expected) => {
      const result = formatCompactDate(value as string | Date);
      expect(result).toBe(expected);
    });
  });

  describe("truncateText", () => {
    it.each([
      [
        "truncate long text",
        "This is a very long text that needs to be truncated",
        20,
        "This is a very long...",
      ],
      ["not truncate short text", "Short text", 20, "Short text"],
      [
        "handle exact length",
        "Exactly 20 chars txt",
        20,
        "Exactly 20 chars txt",
      ],
      [
        "trim whitespace before adding ellipsis",
        "Text with spaces   that needs truncation",
        16,
        "Text with spaces...",
      ],
      ["handle very short maxLength", "Hello World", 5, "Hello..."],
    ])("should %s", (_label, text, maxLength, expected) => {
      const result = truncateText(text, maxLength);
      expect(result).toBe(expected);
    });
  });

  describe("calculateDiscount", () => {
    it.each([
      ["calculate percentage discount", 100, 75, 25],
      ["calculate 50% discount", 50, 25, 50],
      ["round to nearest integer", 100, 66.67, 33],
      ["handle zero discount", 50, 50, 0],
      ["handle small discounts", 100, 99, 1],
      ["handle large discounts", 100, 10, 90],
    ])("should %s", (_label, originalPrice, discountedPrice, expected) => {
      const result = calculateDiscount(originalPrice, discountedPrice);
      expect(result).toBe(expected);
    });
  });
});
