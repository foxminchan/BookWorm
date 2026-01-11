"use client";
import { useState } from "react";

import { toast } from "sonner";

import useCreatePublisher from "@workspace/api-hooks/catalog/publishers/useCreatePublisher";
import useDeletePublisher from "@workspace/api-hooks/catalog/publishers/useDeletePublisher";
import usePublishers from "@workspace/api-hooks/catalog/publishers/usePublishers";
import useUpdatePublisher from "@workspace/api-hooks/catalog/publishers/useUpdatePublisher";
import { Button } from "@workspace/ui/components/button";

import { PageHeader } from "@/components/page-header";
import { SimpleDialog } from "@/components/simple-dialog";
import { SimpleTable } from "@/components/simple-table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Publishers", isActive: true },
];

export default function PublishersPage() {
  const { data: publishers = [], isLoading } = usePublishers();
  const createMutation = useCreatePublisher();
  const updateMutation = useUpdatePublisher();
  const deleteMutation = useDeletePublisher();
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<"create" | "update">("create");
  const [dialogPublisherId, setDialogPublisherId] = useState("");

  const handleCreate = async (name: string) => {
    await createMutation.mutateAsync({ name });
    setIsDialogOpen(false);
  };

  const handleUpdate = async (id: string, name: string) => {
    await updateMutation.mutateAsync({ request: { id, name } });
  };

  const handleDelete = async (id: string) => {
    await deleteMutation.mutateAsync(id);
    toast.success("Publisher has been deleted");
  };

  const handleDialogOpen = (mode: "create" | "update", id?: string) => {
    setDialogMode(mode);
    if (mode === "update" && id) {
      setDialogPublisherId(id);
    }
    setIsDialogOpen(true);
  };

  const handleDialogSubmit = async (value: string) => {
    if (dialogMode === "create") {
      await handleCreate(value);
    } else if (dialogMode === "update") {
      await handleUpdate(dialogPublisherId, value);
    }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        title="Publishers Management"
        description="Manage book publishers"
        breadcrumbs={breadcrumbs}
        action={
          <Button onClick={() => handleDialogOpen("create")}>
            Create Publisher
          </Button>
        }
      />

      <SimpleDialog
        open={isDialogOpen}
        onOpenChange={setIsDialogOpen}
        title={
          dialogMode === "create" ? "Create Publisher" : "Update Publisher"
        }
        description="Enter the publisher's name"
        placeholder="Publisher name"
        onSubmit={handleDialogSubmit}
        isLoading={
          createMutation.isPending ||
          (dialogMode === "update" && updateMutation.isPending)
        }
      />

      <SimpleTable
        title="All Publishers"
        description={`Total: ${publishers.length} publishers`}
        items={publishers}
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
