"use client";

import { useFrontendTool } from "@copilotkit/react-core/v2";
import { z } from "zod";

import booksApiClient from "@workspace/api-client/catalog/books";
import type { Book } from "@workspace/types/catalog/books";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import { Spinner } from "@workspace/ui/components/spinner";
import { formatPrice } from "@workspace/utils/format";

export function useBookSearchActions() {
  useFrontendTool({
    name: "searchBooks",
    description:
      "Search for books by title, author, category, or keywords. Returns a list of matching books.",
    parameters: z.object({
      query: z
        .string()
        .describe("The search query (title, author, or keywords)"),
      category: z
        .string()
        .optional()
        .describe("Optional category filter (fiction, non-fiction, etc.)"),
      maxResults: z
        .number()
        .optional()
        .describe("Maximum number of results to return (default 10)"),
    }),
    handler: async ({ query, category, maxResults = 10 }) => {
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
    render: ({ status, result }) => {
      const parsedResult =
        status === "complete" && result
          ? (JSON.parse(result) as {
              results: Book[];
              total: number;
              query: string;
            })
          : null;

      if (status === "executing") {
        return (
          <Card>
            <CardContent className="flex items-center gap-2 py-4">
              <Spinner />
              <span className="text-sm">Searching for books...</span>
            </CardContent>
          </Card>
        );
      }

      if (status === "complete" && parsedResult) {
        return (
          <Card>
            <CardHeader>
              <CardTitle>
                Found {parsedResult.total} book
                {parsedResult.total === 1 ? "" : "s"}
              </CardTitle>
              <CardDescription>
                Search results for &quot;{parsedResult.query}&quot;
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-3">
              {parsedResult.results.slice(0, 5).map((book: Book) => (
                <div
                  key={book.id}
                  className="hover:bg-accent flex gap-3 rounded-lg border p-3 text-sm transition-colors"
                >
                  <div className="flex-1">
                    <div className="font-medium">{book.name}</div>
                    <div className="text-muted-foreground text-xs">
                      {book.authors
                        .map((a) => a.name)
                        .filter(Boolean)
                        .join(", ")}
                    </div>
                    <div className="mt-1 text-xs font-semibold">
                      {formatPrice(book.price)}
                    </div>
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>
        );
      }

      return <></>;
    },
  });
}
