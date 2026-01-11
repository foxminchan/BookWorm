import { PageHeader } from "@/components/page-header";
import { BookForm } from "@/features/books/book-form";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", href: "/books" },
  { label: "Create New Book", isActive: true },
];

export default function NewBookPage() {
  return (
    <div className="space-y-6">
      <PageHeader
        title="Create New Book"
        description="Add a new book to your inventory"
        breadcrumbs={breadcrumbs}
      />
      <BookForm />
    </div>
  );
}
