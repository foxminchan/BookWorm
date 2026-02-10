"use client";

import { useCopilotAction } from "@copilotkit/react-core";

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

import { currencyFormatter } from "@/lib/constants";

export function useBookSearchActions() {
  useCopilotAction({
    name: "searchBooks",
    description:
      "Search for books by title, author, category, or keywords. Returns a list of matching books.",
    parameters: [
      {
        name: "query",
        type: "string",
        description: "The search query (title, author, or keywords)",
        required: true,
      },
      {
        name: "category",
        type: "string",
        description: "Optional category filter (fiction, non-fiction, etc.)",
        required: false,
      },
      {
        name: "maxResults",
        type: "number",
        description: "Maximum number of results to return (default 10)",
        required: false,
      },
    ],
    handler: async ({ query, category, maxResults = 10 }) => {
      const data = await booksApiClient.list({
        search: query,
        ...(category && { category }),
        pageSize: maxResults,
      });

      return {
        results: data.items || [],
        total: data.totalCount || 0,
        query,
      };
    },
    render: ({ status, result }) => {
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

      if (status === "complete" && result) {
        return (
          <Card>
            <CardHeader>
              <CardTitle>
                Found {result.total} book{result.total === 1 ? "" : "s"}
              </CardTitle>
              <CardDescription>
                Search results for &quot;{result.query}&quot;
              </CardDescription>
            </CardHeader>
            <CardContent className="grid gap-3">
              {result.results.slice(0, 5).map((book: Book) => (
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
                      {currencyFormatter.format(book.price)}
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
