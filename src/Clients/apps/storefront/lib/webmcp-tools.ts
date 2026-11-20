import { z } from "zod";

import booksApiClient from "@workspace/api-client/catalog/books";

export const searchBooksParameters = z.object({
  query: z.string().describe("The search query (title, author, or keywords)"),
  category: z
    .string()
    .optional()
    .describe("Optional category filter (fiction, non-fiction, etc.)"),
  maxResults: z
    .number()
    .optional()
    .describe("Maximum number of results to return (default 10)"),
});

export type SearchBooksArgs = z.infer<typeof searchBooksParameters>;

/**
 * Shared `searchBooks` tool definition used by both `useBookSearchActions`
 * (when CopilotKit chat is active) and standalone `CopilotKitCore` WebMCP
 * registration (when no chat agent is mounted).
 */
export const searchBooksTool = {
  name: "searchBooks",
  description:
    "Search for books by title, author, category, or keywords. Returns a list of matching books.",
  parameters: searchBooksParameters,
  handler: async ({
    query,
    category,
    maxResults = 10,
  }: SearchBooksArgs): Promise<string> => {
    const data = await booksApiClient.list({
      search: query,
      ...(category && { category }),
      pageSize: maxResults,
    });

    return JSON.stringify({
      results: data.items || [],
      total: data.totalCount || 0,
      query,
    });
  },
  webmcp: {
    annotations: {
      readOnlyHint: true,
    },
  },
} as const;

export const getBookDetailsParameters = z.object({
  bookId: z.string().describe("The unique identifier of the book to retrieve"),
});

export type GetBookDetailsArgs = z.infer<typeof getBookDetailsParameters>;

/**
 * Shared `getBookDetails` tool definition used by both `useBookSearchActions`
 * (when CopilotKit chat is active) and standalone `CopilotKitCore` WebMCP
 * registration (when no chat agent is mounted).
 */
export const getBookDetailsTool = {
  name: "getBookDetails",
  description:
    "Retrieve detailed information about a specific book by its ID, including description, price, authors, and rating.",
  parameters: getBookDetailsParameters,
  handler: async ({ bookId }: GetBookDetailsArgs): Promise<string> => {
    const book = await booksApiClient.get(bookId);
    return JSON.stringify(book);
  },
  webmcp: {
    annotations: {
      readOnlyHint: true,
    },
  },
} as const;
