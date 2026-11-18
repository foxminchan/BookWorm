"use client";

import { useFrontendTool } from "@copilotkit/react-core/v2";

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

import { searchBooksTool } from "@/lib/webmcp-tools";

export function useBookSearchActions() {
  useFrontendTool({
    ...searchBooksTool,
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
