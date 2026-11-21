"use client";

import { useCallback, useEffect, useRef, useState } from "react";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";

import { Plus } from "lucide-react";

import type { ListBooksQuery } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";

import { PageContainer } from "@/components/page-container";
import { BooksFilters } from "@/features/books/books-filters";
import { BooksTable } from "@/features/books/table/table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", isActive: true },
];

export function BooksListing() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const newBookId = searchParams.get("new");
  const hasCleanedUrl = useRef(false);

  const [query, setQuery] = useState<
    Omit<ListBooksQuery, "pageIndex" | "pageSize">
  >({});
  const [highlightedBookId, setHighlightedBookId] = useState<string | null>(
    newBookId,
  );

  useEffect(() => {
    if (newBookId && !hasCleanedUrl.current) {
      hasCleanedUrl.current = true;
      // Clean the URL parameter without a full navigation
      router.replace("/books", { scroll: false });
      // Remove highlight after 3 seconds
      const timeout = setTimeout(() => setHighlightedBookId(null), 3000);
      return () => clearTimeout(timeout);
    }
  }, [newBookId, router]);

  const handleFiltersChange = useCallback(
    (newQuery: Omit<ListBooksQuery, "pageIndex" | "pageSize">) => {
      setQuery(newQuery);
    },
    [],
  );

  return (
    <PageContainer
      title="Books Management"
      description="Manage your book inventory"
      breadcrumbs={breadcrumbs}
      action={
        <Button
          render={
            <Link href="/books/new">
              <Plus className="h-4 w-4" aria-hidden="true" />
              Add New Book
            </Link>
          }
          nativeButton={false}
          className="gap-2"
        />
      }
    >
      <BooksFilters onFiltersChange={handleFiltersChange} />
      <BooksTable query={query} highlightedBookId={highlightedBookId} />
    </PageContainer>
  );
}
