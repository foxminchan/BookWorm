import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { PageContainer } from "@/components/page-container";
import { APP_CONFIG } from "@/config/app-config";
import { BookForm } from "@/features/books/book-form";

export const metadata: Metadata = createMetadata({
  title: "Create Book",
  description: "Add a new book to your inventory",
  appName: APP_CONFIG.name,
});

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", href: "/books" },
  { label: "Create New Book", isActive: true },
];

export default function NewBookPage() {
  return (
    <PageContainer
      title="Create New Book"
      description="Add a new book to your inventory"
      breadcrumbs={breadcrumbs}
    >
      <BookForm />
    </PageContainer>
  );
}
