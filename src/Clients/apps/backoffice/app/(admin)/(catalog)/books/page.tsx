"use client";

import { useEffect, useState } from "react";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";

import { Plus } from "lucide-react";

import type { ListBooksQuery } from "@workspace/types/catalog/books";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { BooksFilters } from "@/features/books/books-filters";
import { BooksTable } from "@/features/books/table/table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", isActive: true },
];

export default function BooksPage() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [query, setQuery] = useState<
    Omit<ListBooksQuery, "pageIndex" | "pageSize">
  >({});
  const newBookId = searchParams.get("new");
  const [highlightedBookId, setHighlightedBookId] = useState<string | null>(
    newBookId,
  );

  useEffect(() => {
    if (newBookId) {
      // Clear the URL parameter
      router.replace("/books", { scroll: false });
      // Remove highlight after 3 seconds
      const timeout = setTimeout(() => setHighlightedBookId(null), 3000);
      return () => clearTimeout(timeout);
    }
  }, [newBookId, router]);

  return (
    <div className="space-y-6">
      <PageHeader
        title="Books Management"
        description="Manage your book inventory - 51 total books"
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
      <BooksFilters onFiltersChange={setQuery} />
      <BooksTable query={query} highlightedBookId={highlightedBookId} />
    </div>
  );
}
