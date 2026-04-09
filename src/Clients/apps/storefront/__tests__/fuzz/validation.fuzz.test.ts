import fc from "fast-check";
import { describe, expect, it } from "vitest";

import { basketItemRequestSchema } from "@workspace/validations/basket";
import {
  createAuthorSchema,
  updateAuthorSchema,
} from "@workspace/validations/catalog/authors";
import {
  createBookSchema,
  updateBookSchema,
} from "@workspace/validations/catalog/books";

describe("validation schemas (property-based)", () => {
  describe("createBookSchema", () => {
    it("should never throw on arbitrary input", () => {
      fc.assert(
        fc.property(
          fc.record({
            name: fc.string(),
            description: fc.string(),
            price: fc.double({ noNaN: true }),
            priceSale: fc.option(fc.double({ noNaN: true }), {
              nil: undefined,
            }),
            categoryId: fc.string(),
            publisherId: fc.string(),
            authorIds: fc.array(fc.string()),
          }),
          (input) => {
            const result = createBookSchema.safeParse(input);
            expect(typeof result.success).toBe("boolean");
          },
        ),
      );
    });

    it("should accept valid book data", () => {
      fc.assert(
        fc.property(
          fc.record({
            name: fc.string({ minLength: 1, maxLength: 50 }),
            description: fc.string({ minLength: 1, maxLength: 500 }),
            price: fc.double({ min: 0.01, max: 10_000, noNaN: true }),
            priceSale: fc.constant(null),
            categoryId: fc.uuid(),
            publisherId: fc.uuid(),
            authorIds: fc.array(fc.uuid(), { minLength: 1, maxLength: 5 }),
          }),
          (input) => {
            const result = createBookSchema.safeParse(input);
            expect(result.success).toBe(true);
          },
        ),
      );
    });

    it("should reject negative prices", () => {
      fc.assert(
        fc.property(
          fc.double({ min: -100_000, max: -0.01, noNaN: true }),
          (price) => {
            const result = createBookSchema.safeParse({
              name: "Test Book",
              description: "A test description",
              price,
              categoryId: "cat-1",
              publisherId: "pub-1",
              authorIds: ["author-1"],
            });
            expect(result.success).toBe(false);
          },
        ),
      );
    });

    it("should reject empty author arrays", () => {
      fc.assert(
        fc.property(
          fc.record({
            name: fc.string({ minLength: 1, maxLength: 50 }),
            description: fc.string({ minLength: 1, maxLength: 500 }),
            price: fc.double({ min: 0.01, max: 10_000, noNaN: true }),
            categoryId: fc.uuid(),
            publisherId: fc.uuid(),
          }),
          (input) => {
            const result = createBookSchema.safeParse({
              ...input,
              authorIds: [],
            });
            expect(result.success).toBe(false);
          },
        ),
      );
    });
  });

  describe("updateBookSchema", () => {
    it("should never throw on arbitrary input", () => {
      fc.assert(
        fc.property(fc.anything(), (input) => {
          const result = updateBookSchema.safeParse(input);
          expect(typeof result.success).toBe("boolean");
        }),
      );
    });

    it("should reject non-UUID id values", () => {
      fc.assert(
        fc.property(
          fc.string().filter((s) => !isUuid(s)),
          (id) => {
            const result = updateBookSchema.safeParse({
              id,
              name: "Test",
              description: "Test desc",
              price: 10,
              categoryId: "cat-1",
              publisherId: "pub-1",
              authorIds: ["00000000-0000-0000-0000-000000000001"],
            });
            expect(result.success).toBe(false);
          },
        ),
      );
    });
  });

  describe("createAuthorSchema", () => {
    it("should never throw on arbitrary input", () => {
      fc.assert(
        fc.property(fc.anything(), (input) => {
          const result = createAuthorSchema.safeParse(input);
          expect(typeof result.success).toBe("boolean");
        }),
      );
    });

    it("should reject names exceeding max length", () => {
      fc.assert(
        fc.property(fc.string({ minLength: 101, maxLength: 500 }), (name) => {
          const result = createAuthorSchema.safeParse({ name });
          expect(result.success).toBe(false);
        }),
      );
    });
  });

  describe("updateAuthorSchema", () => {
    it("should accept valid UUID + name pairs", () => {
      fc.assert(
        fc.property(
          fc.uuid(),
          fc.string({ minLength: 1, maxLength: 100 }),
          (id, name) => {
            const result = updateAuthorSchema.safeParse({ id, name });
            expect(result.success).toBe(true);
          },
        ),
      );
    });
  });

  describe("basketItemRequestSchema", () => {
    it("should never throw on arbitrary input", () => {
      fc.assert(
        fc.property(fc.anything(), (input) => {
          const result = basketItemRequestSchema.safeParse(input);
          expect(typeof result.success).toBe("boolean");
        }),
      );
    });

    it("should reject non-positive quantities", () => {
      fc.assert(
        fc.property(fc.integer({ min: -1000, max: 0 }), (quantity) => {
          const result = basketItemRequestSchema.safeParse({
            id: "item-1",
            quantity,
          });
          expect(result.success).toBe(false);
        }),
      );
    });

    it("should accept valid basket items", () => {
      fc.assert(
        fc.property(
          fc.string({ minLength: 1 }),
          fc.integer({ min: 1, max: 10_000 }),
          (id, quantity) => {
            const result = basketItemRequestSchema.safeParse({ id, quantity });
            expect(result.success).toBe(true);
          },
        ),
      );
    });
  });
});

function isUuid(s: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-7][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(
    s,
  );
}
