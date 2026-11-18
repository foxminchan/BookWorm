import { beforeEach, describe, expect, it, vi } from "vitest";

import { searchBooksParameters, searchBooksTool } from "@/lib/webmcp-tools";

const { mockList } = vi.hoisted(() => ({
  mockList: vi.fn(),
}));

vi.mock("@workspace/api-client/catalog/books", () => ({
  default: {
    list: mockList,
  },
}));

describe("searchBooksTool", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("uses default maxResults and omits category when not provided", async () => {
    mockList.mockResolvedValue({
      items: [{ id: "1", name: "Clean Code" }],
      totalCount: 1,
    });

    const result = await searchBooksTool.handler({ query: "clean code" });

    expect(mockList).toHaveBeenCalledWith({
      search: "clean code",
      pageSize: 10,
    });
    expect(result).toBe(
      JSON.stringify({
        results: [{ id: "1", name: "Clean Code" }],
        total: 1,
        query: "clean code",
      }),
    );
  });

  it("includes category and custom maxResults when provided", async () => {
    mockList.mockResolvedValue({
      items: [{ id: "2", name: "Dune" }],
      totalCount: 1,
    });

    const result = await searchBooksTool.handler({
      query: "dune",
      category: "fiction",
      maxResults: 5,
    });

    expect(mockList).toHaveBeenCalledWith({
      search: "dune",
      category: "fiction",
      pageSize: 5,
    });
    expect(result).toBe(
      JSON.stringify({
        results: [{ id: "2", name: "Dune" }],
        total: 1,
        query: "dune",
      }),
    );
  });

  it("falls back to empty results and zero total when API response omits them", async () => {
    mockList.mockResolvedValue({});

    const result = await searchBooksTool.handler({ query: "unknown" });

    expect(result).toBe(
      JSON.stringify({
        results: [],
        total: 0,
        query: "unknown",
      }),
    );
  });

  it("validates parameters with searchBooksParameters", () => {
    expect(searchBooksParameters.parse({ query: "ddd" })).toEqual({
      query: "ddd",
    });
    expect(
      searchBooksParameters.parse({
        query: "ddd",
        category: "architecture",
        maxResults: 20,
      }),
    ).toEqual({
      query: "ddd",
      category: "architecture",
      maxResults: 20,
    });
    expect(() => searchBooksParameters.parse({})).toThrow();
  });
});
