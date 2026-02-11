"use client";

import useCategories from "@workspace/api-hooks/catalog/categories/useCategories";
import useCreateCategory from "@workspace/api-hooks/catalog/categories/useCreateCategory";
import useDeleteCategory from "@workspace/api-hooks/catalog/categories/useDeleteCategory";
import useUpdateCategory from "@workspace/api-hooks/catalog/categories/useUpdateCategory";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";
import { useCrudPage } from "@/hooks/use-crud-page";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Categories", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export default function CategoriesPage() {
  const {
    items: categories,
    isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useCrudPage({
    entityName: "Category",
    listQuery: useCategories(),
    createMutation: useCreateCategory(),
    updateMutation: useUpdateCategory(),
    deleteMutation: useDeleteCategory(),
    buildCreateRequest,
    buildUpdateRequest,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Categories Management"
        description="Manage book categories"
        breadcrumbs={breadcrumbs}
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
        isLoading={isCreatePending}
      />

      <SimpleTable
        title="All Categories"
        description={`Total: ${categories.length} categories`}
        items={categories}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={isSubmitting}
      />
    </div>
  );
}
