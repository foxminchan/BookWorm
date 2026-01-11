"use client";

import { useState } from "react";

import { PageHeader } from "@/components/page-header";
import { BookSelector } from "@/features/reviews/book-selector";
import { ReviewsTable } from "@/features/reviews/table/table";

export default function ReviewsPage() {
  const [selectedBookId, setSelectedBookId] = useState<string>("");

  return (
    <div className="space-y-6">
      <PageHeader
        title="Reviews Management"
        description="Moderate customer reviews for your books"
        breadcrumbs={[
          { label: "Admin", href: "/" },
          { label: "Reviews", isActive: true },
        ]}
      />

      <div className="border-border bg-card rounded-lg border p-6">
        <BookSelector
          onBookSelect={setSelectedBookId}
          selectedBookId={selectedBookId}
        />
      </div>

      {selectedBookId && <ReviewsTable bookId={selectedBookId} />}

      {!selectedBookId && (
        <div className="border-border bg-muted/50 flex items-center justify-center rounded-lg border border-dashed p-12 text-center">
          <p className="text-muted-foreground">
            Select a book above to view and manage its reviews
          </p>
        </div>
      )}
    </div>
  );
}
