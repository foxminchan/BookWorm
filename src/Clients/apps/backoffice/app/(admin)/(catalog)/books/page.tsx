"use client";

import { Suspense, useCallback, useEffect, useRef, useState } from "react";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";

import { Plus } from "lucide-react";

import type { ListBooksQuery } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";
import { Skeleton } from "@workspace/ui/components/skeleton";

import { PageHeader } from "@/components/page-header";
import { BooksFilters } from "@/features/books/books-filters";
import { BooksTable } from "@/features/books/table/table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", isActive: true },
];

function BooksPageContent() {
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
    <div className="space-y-6">
      <PageHeader
        title="Books Management"
        description="Manage your book inventory"
        breadcrumbs={breadcrumbs}
        action={
          <Link href="/books/new">
            <Button className="gap-2">
              <Plus className="h-4 w-4" />
              Add New Book
            </Button>
          </Link>
        }
      />
      <BooksFilters onFiltersChange={handleFiltersChange} />
      <BooksTable query={query} highlightedBookId={highlightedBookId} />
    </div>
  );
}

function BooksPageSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-32 w-full" />
      <Skeleton className="h-64 w-full" />
    </div>
  );
}

export default function BooksPage() {
  return (
    <Suspense fallback={<BooksPageSkeleton />}>
      <BooksPageContent />
    </Suspense>
  );
}
