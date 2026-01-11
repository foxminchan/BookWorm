import { PageHeader } from "@/components/page-header";
import { BookForm } from "@/features/books/book-form";

type EditBookPageProps = {
  params: Promise<{ id: string }>;
};

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Books", href: "/books" },
  { label: "Edit Book", isActive: true },
];

export default async function EditBookPage({ params }: EditBookPageProps) {
  const { id } = await params;

  return (
    <div className="space-y-6">
      <PageHeader
        title="Edit Book"
        description="Modify the details of the book"
        breadcrumbs={breadcrumbs}
      />
      <BookForm bookId={id} />
    </div>
  );
}
