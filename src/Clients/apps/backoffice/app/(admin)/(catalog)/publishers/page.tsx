"use client";

import useCreatePublisher from "@workspace/api-hooks/catalog/publishers/useCreatePublisher";
import useDeletePublisher from "@workspace/api-hooks/catalog/publishers/useDeletePublisher";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import useUpdatePublisher from "@workspace/api-hooks/catalog/publishers/useUpdatePublisher";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";
import { useCrudPage } from "@/hooks/use-crud-page";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Publishers", isActive: true },
];

const buildCreateRequest = (name: string) => ({ name });
const buildUpdateRequest = (id: string, name: string) => ({
  request: { id, name },
});

export default function PublishersPage() {
  const {
    items: publishers,
    isLoading,
    isDialogOpen,
    setIsDialogOpen,
    isSubmitting,
    isCreatePending,
    handleCreate,
    handleUpdate,
    handleDelete,
  } = useCrudPage({
    entityName: "Publisher",
    listQuery: usePublishers(),
    createMutation: useCreatePublisher(),
    updateMutation: useUpdatePublisher(),
    deleteMutation: useDeletePublisher(),
    buildCreateRequest,
    buildUpdateRequest,
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Publishers Management"
        description="Manage book publishers"
        breadcrumbs={breadcrumbs}
        action={
          <Button onClick={() => setIsDialogOpen(true)}>
            Create Publisher
          </Button>
        }
      />

      <SimpleDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title="Create Publisher"
        description="Enter the publisher's name"
        placeholder="Publisher name"
        onSubmit={handleCreate}
        isLoading={isCreatePending}
      />

      <SimpleTable
        title="All Publishers"
        description={`Total: ${publishers.length} publishers`}
        items={publishers}
        isLoading={isLoading}
        onUpdate={handleUpdate}
        onDelete={handleDelete}
        isSubmitting={isSubmitting}
      />
    </div>
  );
}
