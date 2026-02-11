"use client";

import { useCallback, useMemo, useState } from "react";

import Link from "next/link";

import { CheckCircle2, Eye, Trash2, XCircle } from "lucide-react";
import { toast } from "sonner";

import useCancelOrder from "@workspace/api-hooks/ordering/orders/useCancelOrder";
import useCompleteOrder from "@workspace/api-hooks/ordering/orders/useCompleteOrder";
import useDeleteOrder from "@workspace/api-hooks/ordering/orders/useDeleteOrder";
import type { Order } from "@workspace/types/ordering/orders";
import { Button } from "@workspace/ui/components/button";

import { ConfirmDialog } from "@/components/confirm-dialog";
import {
  type OrderStatus,
  canCancelOrder,
  canCompleteOrder,
} from "@/lib/pattern";

type CellActionProps = Readonly<{
  order: Order;
}>;

export function CellAction({ order }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const [openComplete, setOpenComplete] = useState(false);
  const [openCancel, setOpenCancel] = useState(false);
  const completeOrderMutation = useCompleteOrder();
  const cancelOrderMutation = useCancelOrder();
  const deleteOrderMutation = useDeleteOrder();

  const status = order.status as OrderStatus;
  const canComplete = canCompleteOrder(status);
  const canCancel = canCancelOrder(status);

  const orderIdShort = useMemo(() => order.id.slice(0, 8), [order.id]);

  const handleComplete = useCallback(async () => {
    completeOrderMutation.mutate(order.id, {
      onSuccess: () => {
        setOpenComplete(false);
        toast.success("Order has been completed");
      },
    });
  }, [order.id, completeOrderMutation]);

  const handleCancel = useCallback(async () => {
    cancelOrderMutation.mutate(order.id, {
      onSuccess: () => {
        setOpenCancel(false);
        toast.info("Order has been canceled");
      },
    });
  }, [order.id, cancelOrderMutation]);

  const handleDelete = useCallback(async () => {
    deleteOrderMutation.mutate(order.id, {
      onSuccess: () => {
        setOpenDelete(false);
        toast.success("Order has been deleted");
      },
    });
  }, [order.id, deleteOrderMutation]);

  const confirmDialogs = useMemo(
    () => [
      {
        open: openComplete,
        onOpenChange: setOpenComplete,
        title: "Complete Order",
        description: `Are you sure you want to mark order #${orderIdShort} as completed? This action cannot be undone.`,
        actionLabel: "Complete",
        actionType: "complete" as const,
        isLoading: completeOrderMutation.isPending,
        onConfirm: handleComplete,
      },
      {
        open: openCancel,
        onOpenChange: setOpenCancel,
        title: "Cancel Order",
        description: `Are you sure you want to cancel order #${orderIdShort}? This action cannot be undone.`,
        actionLabel: "Cancel",
        actionType: "cancel" as const,
        isLoading: cancelOrderMutation.isPending,
        onConfirm: handleCancel,
      },
      {
        open: openDelete,
        onOpenChange: setOpenDelete,
        title: "Delete Order",
        description: `Are you sure you want to delete order #${orderIdShort}? This action cannot be undone.`,
        actionLabel: "Delete",
        actionType: "delete" as const,
        isLoading: deleteOrderMutation.isPending,
        onConfirm: handleDelete,
      },
    ],
    [
      openComplete,
      openCancel,
      openDelete,
      orderIdShort,
      completeOrderMutation.isPending,
      cancelOrderMutation.isPending,
      deleteOrderMutation.isPending,
      handleComplete,
      handleCancel,
      handleDelete,
    ],
  );

  return (
    <>
      <div className="flex items-center justify-end gap-2">
        {canComplete && canCancel && (
          <>
            <Button
              variant="outline"
              size="sm"
              className="gap-1 bg-transparent text-green-600 hover:text-green-700"
              onClick={() => setOpenComplete(true)}
              disabled={completeOrderMutation.isPending}
              aria-label={`Complete order ${orderIdShort}`}
            >
              <CheckCircle2 className="h-4 w-4" aria-hidden="true" />
              {completeOrderMutation.isPending ? "Processing..." : "Complete"}
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="gap-1 bg-transparent text-orange-600 hover:text-orange-700"
              onClick={() => setOpenCancel(true)}
              disabled={cancelOrderMutation.isPending}
              aria-label={`Cancel order ${orderIdShort}`}
            >
              <XCircle className="h-4 w-4" aria-hidden="true" />
              {cancelOrderMutation.isPending ? "Processing..." : "Cancel"}
            </Button>
          </>
        )}
        <Button variant="ghost" size="sm" asChild>
          <Link
            href={`/orders/${order.id}`}
            aria-label={`View order details ${orderIdShort}`}
          >
            <Eye className="h-4 w-4" aria-hidden="true" />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteOrderMutation.isPending}
          aria-label={`Delete order ${orderIdShort}`}
        >
          <Trash2 className="h-4 w-4" aria-hidden="true" />
        </Button>
      </div>

      {confirmDialogs.map((dialog) => (
        <ConfirmDialog key={dialog.title} {...dialog} />
      ))}
    </>
  );
}
