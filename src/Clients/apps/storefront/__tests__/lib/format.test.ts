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
    it("should format price with dollar sign", () => {
      expect(formatPrice(19.99)).toBe("$19.99");
    });

    it("should format whole numbers", () => {
      expect(formatPrice(50)).toBe("$50.00");
    });

    it("should format large numbers with commas", () => {
      expect(formatPrice(1234.56)).toBe("$1,234.56");
    });

    it("should handle zero", () => {
      expect(formatPrice(0)).toBe("$0.00");
    });

    it("should handle negative numbers", () => {
      expect(formatPrice(-15.5)).toBe("-$15.50");
    });
  });

  describe("formatDate", () => {
    it("should format date string", () => {
      const result = formatDate("2024-01-15");
      expect(result).toBe("January 15, 2024");
    });

    it("should format Date object", () => {
      const date = new Date("2024-12-25");
      const result = formatDate(date);
      expect(result).toBe("December 25, 2024");
    });

    it("should handle ISO date strings", () => {
      const result = formatDate("2024-03-10T10:30:00Z");
      expect(result).toContain("March");
      expect(result).toContain("2024");
    });
  });

  describe("formatCompactDate", () => {
    it("should format date in compact style", () => {
      const result = formatCompactDate("2024-01-15");
      expect(result).toBe("Jan 15, 2024");
    });

    it("should format Date object compactly", () => {
      const date = new Date("2024-12-25");
      const result = formatCompactDate(date);
      expect(result).toBe("Dec 25, 2024");
    });

    it("should use abbreviated month names", () => {
      const result = formatCompactDate("2024-09-01");
      expect(result).toBe("Sep 1, 2024");
    });
  });

  describe("truncateText", () => {
    it("should truncate long text", () => {
      const text = "This is a very long text that needs to be truncated";
      const result = truncateText(text, 20);
      expect(result).toBe("This is a very long...");
    });

    it("should not truncate short text", () => {
      const text = "Short text";
      const result = truncateText(text, 20);
      expect(result).toBe("Short text");
    });

    it("should handle exact length", () => {
      const text = "Exactly 20 chars txt";
      const result = truncateText(text, 20);
      expect(result).toBe("Exactly 20 chars txt");
    });

    it("should trim whitespace before adding ellipsis", () => {
      const text = "Text with spaces   that needs truncation";
      const result = truncateText(text, 16);
      expect(result).toBe("Text with spaces...");
    });

    it("should handle very short maxLength", () => {
      const text = "Hello World";
      const result = truncateText(text, 5);
      expect(result).toBe("Hello...");
    });
  });

  describe("calculateDiscount", () => {
    it("should calculate percentage discount", () => {
      const result = calculateDiscount(100, 75);
      expect(result).toBe(25);
    });

    it("should calculate 50% discount", () => {
      const result = calculateDiscount(50, 25);
      expect(result).toBe(50);
    });

    it("should round to nearest integer", () => {
      const result = calculateDiscount(100, 66.67);
      expect(result).toBe(33);
    });

    it("should handle zero discount", () => {
      const result = calculateDiscount(50, 50);
      expect(result).toBe(0);
    });

    it("should handle small discounts", () => {
      const result = calculateDiscount(100, 99);
      expect(result).toBe(1);
    });

    it("should handle large discounts", () => {
      const result = calculateDiscount(100, 10);
      expect(result).toBe(90);
    });
  });
});
