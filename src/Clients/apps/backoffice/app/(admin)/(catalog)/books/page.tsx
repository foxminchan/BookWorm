import { Suspense } from "react";

import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";
import { Skeleton } from "@workspace/ui/components/skeleton";

import { APP_CONFIG } from "@/config/app-config";
import { BooksListing } from "@/features/books/books-listing";

export const metadata: Metadata = createMetadata({
  title: "Books",
  description: "Manage your book inventory",
  appName: APP_CONFIG.name,
});

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
      <BooksListing />
    </Suspense>
  );
}
