import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

import booksApiClient from "@workspace/api-client/catalog/books";

import {
  getBookDetailsParameters,
  getBookDetailsTool,
  searchBooksParameters,
  searchBooksTool,
} from "@/lib/webmcp-tools";

describe("searchBooksTool", () => {
  let listSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    vi.clearAllMocks();
    listSpy = vi.spyOn(booksApiClient, "list");
  });

  afterEach(() => {
    listSpy.mockRestore();
  });

  it("uses default maxResults and omits category when not provided", async () => {
    listSpy.mockResolvedValue({
      items: [{ id: "1", name: "Clean Code" }],
      totalCount: 1,
    } as never);

    const result = await searchBooksTool.handler({ query: "clean code" });

    expect(listSpy).toHaveBeenCalledWith({
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
    listSpy.mockResolvedValue({
      items: [{ id: "2", name: "Dune" }],
      totalCount: 1,
    } as never);

    const result = await searchBooksTool.handler({
      query: "dune",
      category: "fiction",
      maxResults: 5,
    });

    expect(listSpy).toHaveBeenCalledWith({
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
    listSpy.mockResolvedValue({} as never);

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

describe("getBookDetailsTool", () => {
  let getSpy: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    vi.clearAllMocks();
    getSpy = vi.spyOn(booksApiClient, "get");
  });

  afterEach(() => {
    getSpy.mockRestore();
  });

  it("retrieves book details by ID and returns serialized JSON", async () => {
    const book = { id: "book-1", name: "Dune", price: 19.99 };
    getSpy.mockResolvedValue(book as never);

    const result = await getBookDetailsTool.handler({ bookId: "book-1" });

    expect(getSpy).toHaveBeenCalledWith("book-1");
    expect(result).toBe(JSON.stringify(book));
  });

  it("validates parameters with getBookDetailsParameters", () => {
    expect(getBookDetailsParameters.parse({ bookId: "book-1" })).toEqual({
      bookId: "book-1",
    });
    expect(() => getBookDetailsParameters.parse({})).toThrow();
  });
});
