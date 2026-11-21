import type { Metadata } from "next";

import { createMetadata } from "@workspace/seo/metadata";

import { PageContainer } from "@/components/page-container";
import { APP_CONFIG } from "@/config/app-config";
import { BookForm } from "@/features/books/book-form";

export const metadata: Metadata = createMetadata({
  title: "Edit Book",
  description: "Modify the details of the book",
  appName: APP_CONFIG.name,
});

type EditBookPageProps = {
  params: Promise<{ id: string }>;
};

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", href: "/books" },
  { label: "Edit Book", isActive: true },
];

export default async function EditBookPage({
  params,
}: Readonly<EditBookPageProps>) {
  const { id } = await params;

  return (
    <PageContainer
      title="Edit Book"
      description="Modify the details of the book"
      breadcrumbs={breadcrumbs}
    >
      <BookForm bookId={id} />
    </PageContainer>
  );
}
