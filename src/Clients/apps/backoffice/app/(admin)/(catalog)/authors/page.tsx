"use client";
import { useState } from "react";

import useAuthors from "@workspace/api-hooks/catalog/authors/useAuthors";
import useCreateAuthor from "@workspace/api-hooks/catalog/authors/useCreateAuthor";
import useDeleteAuthor from "@workspace/api-hooks/catalog/authors/useDeleteAuthor";
import useUpdateAuthor from "@workspace/api-hooks/catalog/authors/useUpdateAuthor";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";

export default function AuthorsPage() {
  const { data: authors = [], isLoading } = useAuthors();
  const createMutation = useCreateAuthor();
  const updateMutation = useUpdateAuthor();
  const deleteMutation = useDeleteAuthor();
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const handleCreate = async (name: string) => {
    await createMutation.mutateAsync({ name });
    setIsDialogOpen(false);
  };

  const handleUpdate = async (id: string, name: string) => {
    await updateMutation.mutateAsync({ request: { id, name } });
  };

  const handleDelete = async (id: string) => {
    await deleteMutation.mutateAsync(id);
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Authors Management"
        description="Manage book authors"
        breadcrumbs={[
          { label: "Admin", href: "/" },
          { label: "Authors", isActive: true },
        ]}
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
        isLoading={createMutation.isPending}
      />

      <SimpleTable
        title="All Authors"
        description={`Total: ${authors.length} authors`}
        items={authors}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={
          createMutation.isPending ||
          updateMutation.isPending ||
          deleteMutation.isPending
        }
      />
    </div>
  );
}
