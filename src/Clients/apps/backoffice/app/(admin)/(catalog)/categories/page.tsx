"use client";

import { useState } from "react";

import { toast } from "sonner";

import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import useCreateCategory from "@workspace/api-hooks/catalog/categories/useCreateCategory";
import useDeleteCategory from "@workspace/api-hooks/catalog/categories/useDeleteCategory";
import useUpdateCategory from "@workspace/api-hooks/catalog/categories/useUpdateCategory";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";

export default function CategoriesPage() {
  const { data: categories = [], isLoading } = useCategories();
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const deleteMutation = useDeleteCategory();
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  const handleCreate = async (name: string) => {
    await createMutation.mutateAsync({ name });
    setIsDialogOpen(false);
  };

  const handleUpdate = async (id: string, name: string) => {
    await updateMutation.mutateAsync({ request: { id, name } });
  };

  const handleDelete = async (id: string) => {
    await deleteMutation.mutateAsync(id, {
      onSuccess: () => {
        toast.success("Category has been deleted");
      },
    });
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Categories Management"
        description="Manage book categories"
        breadcrumbs={[
          { label: "Admin", href: "/" },
          { label: "Categories", isActive: true },
        ]}
        action={
          <Button onClick={() => setIsDialogOpen(true)}>Create Category</Button>
        }
      />

      <SimpleDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title="Create Category"
        description="Enter the category name"
        placeholder="Category name"
        onSubmit={handleCreate}
        isLoading={createMutation.isPending}
      />

      <SimpleTable
        title="All Categories"
        description={`Total: ${categories.length} categories`}
        items={categories}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={updateMutation.isPending || deleteMutation.isPending}
      />
    </div>
  );
}
