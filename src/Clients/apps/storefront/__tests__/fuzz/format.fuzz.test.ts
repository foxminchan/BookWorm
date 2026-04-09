import fc from "fast-check";
import { describe, expect, it } from "vitest";

import {
  calculateDiscount,
  formatPrice,
  truncateText,
} from "@workspace/utils/format";

describe("format utils (property-based)", () => {
  describe("formatPrice", () => {
    it("should never throw for any finite number", () => {
      fc.assert(
        fc.property(fc.double({ noNaN: true }), (n) => {
          const result = formatPrice(n);
          expect(typeof result).toBe("string");
        }),
      );
    });

    it("should always contain a dollar sign", () => {
      fc.assert(
        fc.property(fc.double({ min: 0, max: 1_000_000, noNaN: true }), (n) => {
          expect(formatPrice(n)).toContain("$");
        }),
      );
    });
  });

  describe("truncateText", () => {
    it("should never exceed maxLength + ellipsis length", () => {
      fc.assert(
        fc.property(
          fc.string(),
          fc.integer({ min: 0, max: 1000 }),
          (text, maxLength) => {
            const result = truncateText(text, maxLength);
            if (text.length <= maxLength) {
              expect(result).toBe(text);
            } else {
              expect(result.length).toBeLessThanOrEqual(maxLength + 3);
            }
          },
        ),
      );
    });

    it("should return original text when shorter than maxLength", () => {
      fc.assert(
        fc.property(fc.string({ maxLength: 50 }), (text) => {
          expect(truncateText(text, text.length + 10)).toBe(text);
        }),
      );
    });
  });

  describe("calculateDiscount", () => {
    it("should return 0-100 for valid price pairs", () => {
      fc.assert(
        fc.property(
          fc.double({ min: 0.01, max: 100_000, noNaN: true }),
          fc.double({ min: 0, max: 1, noNaN: true }),
          (originalPrice, ratio) => {
            const salePrice = originalPrice * ratio;
            const discount = calculateDiscount(originalPrice, salePrice);
            expect(discount).toBeGreaterThanOrEqual(0);
            expect(discount).toBeLessThanOrEqual(100);
          },
        ),
      );
    });

    it("should return an integer (rounded)", () => {
      fc.assert(
        fc.property(
          fc.double({ min: 1, max: 10_000, noNaN: true }),
          fc.double({ min: 0.01, max: 1, noNaN: true }),
          (originalPrice, ratio) => {
            const salePrice = originalPrice * ratio;
            const discount = calculateDiscount(originalPrice, salePrice);
            expect(Number.isInteger(discount)).toBe(true);
          },
        ),
      );
    });
  });
});
