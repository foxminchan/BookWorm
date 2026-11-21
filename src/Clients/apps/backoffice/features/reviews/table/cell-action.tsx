"use client";

import { useCallback, useState } from "react";

import { Trash2 } from "lucide-react";
import { toast } from "sonner";

import useDeleteFeedback from "@workspace/api-hooks/rating/useDeleteFeedback";
import type { Feedback } from "@workspace/types/rating";
import { Button } from "@workspace/ui/components/button";

import { ConfirmDialog } from "@/components/confirm-dialog";

type CellActionProps = Readonly<{
  feedback: Feedback;
}>;

export function CellAction({ feedback }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const deleteFeedbackMutation = useDeleteFeedback();

  const { mutateAsync: deleteFeedback } = deleteFeedbackMutation;

  const handleDelete = useCallback(async () => {
    try {
      await deleteFeedback(feedback.id, {
        onSuccess: () => {
          setOpenDelete(false);
          toast.success("Review has been deleted");
        },
        onError: () => {
          toast.error("Failed to delete review");
        },
      });
    } catch (error) {
      console.error("Failed to delete review:", error);
    }
  }, [feedback.id, deleteFeedback]);

  return (
    <>
      <div className="flex items-center justify-end">
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteFeedbackMutation.isPending}
          aria-label={`Delete review with ${feedback.rating} star rating`}
        >
          <Trash2 className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>

      <ConfirmDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        title="Delete Review"
        description={`Are you sure you want to delete this review? The customer gave it a ${feedback.rating} star rating.`}
        actionLabel="Delete"
        actionType="delete"
        isLoading={deleteFeedbackMutation.isPending}
        onConfirm={handleDelete}
      />
    </>
  );
}
