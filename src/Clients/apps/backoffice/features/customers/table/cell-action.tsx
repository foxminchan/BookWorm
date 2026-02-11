"use client";

import { useCallback, useState } from "react";

import { Trash2 } from "lucide-react";
import { toast } from "sonner";

import useDeleteBuyer from "@workspace/api-hooks/ordering/buyers/useDeleteBuyer";
import type { Buyer } from "@workspace/types/ordering/buyers";
import { Button } from "@workspace/ui/components/button";

import { ConfirmDialog } from "@/components/confirm-dialog";

type CellActionProps = Readonly<{
  customer: Buyer;
}>;

export function CellAction({ customer }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const deleteCustomerMutation = useDeleteBuyer();

  const handleDelete = useCallback(async () => {
    deleteCustomerMutation.mutate(customer.id, {
      onSuccess: () => {
        setOpenDelete(false);
        toast.success("Customer has been deleted");
      },
    });
  }, [customer.id, deleteCustomerMutation]);

  return (
    <>
      <div className="flex items-center justify-end gap-2">
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteCustomerMutation.isPending}
          aria-label={`Delete ${customer.name ?? "customer"}`}
        >
          <Trash2 className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>

      <ConfirmDialog
        open={openDelete}
        onOpenChange={setOpenDelete}
        title="Delete Customer"
        description={`Are you sure you want to delete "${customer.name ?? "this customer"}"? This action cannot be undone.`}
        actionLabel="Delete"
        actionType="delete"
        isLoading={deleteCustomerMutation.isPending}
        onConfirm={handleDelete}
      />
    </>
  );
}
