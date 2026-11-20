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

import { getBookDetailsTool, searchBooksTool } from "@/lib/webmcp-tools";

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

  useFrontendTool({
    ...getBookDetailsTool,
    render: ({ status, result }) => {
      const book =
        status === "complete" && result ? (JSON.parse(result) as Book) : null;

      if (status === "executing") {
        return (
          <Card>
            <CardContent className="flex items-center gap-2 py-4">
              <Spinner />
              <span className="text-sm">Loading book details...</span>
            </CardContent>
          </Card>
        );
      }

      if (status === "complete" && book) {
        return (
          <Card>
            <CardHeader>
              <CardTitle>{book.name}</CardTitle>
              <CardDescription>
                {book.authors
                  ?.map((a) => a.name)
                  .filter(Boolean)
                  .join(", ")}
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-2 text-sm">
              {book.description && (
                <p className="text-muted-foreground line-clamp-3">
                  {book.description}
                </p>
              )}
              <div className="font-semibold">
                {formatPrice(book.priceSale ?? book.price)}
              </div>
            </CardContent>
          </Card>
        );
      }

      return <></>;
    },
  });
}
