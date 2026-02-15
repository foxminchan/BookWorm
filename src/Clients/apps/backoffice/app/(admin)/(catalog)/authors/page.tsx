"use client";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useCreateAuthor from "@workspace/api-hooks/catalog/authors/useCreateAuthor";
import useDeleteAuthor from "@workspace/api-hooks/catalog/authors/useDeleteAuthor";
import useUpdateAuthor from "@workspace/api-hooks/catalog/authors/useUpdateAuthor";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";
import { useCrudPage } from "@/hooks/useCrudPage";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Authors", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export default function AuthorsPage() {
  const {
    items: authors,
    isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useCrudPage({
    entityName: "Author",
    listQuery: useAuthors(),
    createMutation: useCreateAuthor(),
    updateMutation: useUpdateAuthor(),
    deleteMutation: useDeleteAuthor(),
    buildCreateRequest,
    buildUpdateRequest,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Authors Management"
        description="Manage book authors"
        breadcrumbs={breadcrumbs}
        action={
          <Button onClick={() => setIsDialogOpen(true)}>Create Author</Button>
        }
      />

      <SimpleDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title="Create Author"
        description="Enter the author's name"
        placeholder="Author name"
        onSubmit={handleCreate}
        isLoading={isCreatePending}
      />

      <SimpleTable
        title="All Authors"
        description={`Total: ${authors.length} authors`}
        items={authors}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={isSubmitting}
      />
    </div>
  );
}
