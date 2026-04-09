import fc from "fast-check";
import { describe, expect, it } from "vitest";

import { buildPaginationLink } from "@workspace/utils/link";
import { formatValidationErrors } from "@workspace/utils/validation";

describe("formatValidationErrors (property-based)", () => {
  it("should always return status 400", () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            path: fc.array(fc.oneof(fc.string(), fc.integer())),
            message: fc.string(),
          }),
        ),
        (issues) => {
          const result = formatValidationErrors({ issues });
          expect(result.status).toBe(400);
          expect(result.title).toBe("One or more validation errors occurred.");
          expect(typeof result.traceId).toBe("string");
        },
      ),
    );
  });

  it("should capitalize field names in errors", () => {
    fc.assert(
      fc.property(
        fc.array(
          fc.record({
            path: fc.tuple(
              fc.string({ minLength: 1 }).filter((s) => /^[a-z]/.test(s)),
            ),
            message: fc.string({ minLength: 1 }),
          }),
          { minLength: 1 },
        ),
        (issues) => {
          const result = formatValidationErrors({ issues });
          for (const key of Object.keys(result.errors)) {
            if (key !== "Unknown") {
              expect(key[0]).toBe(key[0]!.toUpperCase());
            }
          }
        },
      ),
    );
  });

  it("should use 'Unknown' when path is empty", () => {
    fc.assert(
      fc.property(fc.string({ minLength: 1 }), (message) => {
        const result = formatValidationErrors({
          issues: [{ path: [], message }],
        });
        expect(result.errors).toHaveProperty("Unknown");
      }),
    );
  });
});

describe("buildPaginationLink (property-based)", () => {
  it("should always produce a valid link header format", () => {
    fc.assert(
      fc.property(
        fc.webUrl(),
        fc.integer({ min: 1, max: 10_000 }),
        fc.integer({ min: 1, max: 100 }),
        fc.constantFrom("first", "self", "previous", "next", "last"),
        (url, page, pageSize, rel) => {
          const result = buildPaginationLink(url, page, pageSize, rel);
          expect(result).toContain(`rel=${rel}`);
          expect(result).toContain(`pageIndex=${page}`);
          expect(result).toContain(`pageSize=${pageSize}`);
          expect(result.startsWith("<")).toBe(true);
          expect(result).toContain(">;rel=");
        },
      ),
    );
  });
});
