"use client";

import { useState } from "react";

import Link from "next/link";

import { CheckCircle2, Eye, Trash2, XCircle } from "lucide-react";

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

type CellActionProps = {
  order: Order;
};

export function CellAction({ order }: CellActionProps) {
  const [openDelete, setOpenDelete] = useState(false);
  const [openComplete, setOpenComplete] = useState(false);
  const [openCancel, setOpenCancel] = useState(false);
  const completeOrderMutation = useCompleteOrder();
  const cancelOrderMutation = useCancelOrder();
  const deleteOrderMutation = useDeleteOrder(order.id);

  const status = order.status as OrderStatus;
  const canComplete = canCompleteOrder(status);
  const canCancel = canCancelOrder(status);

  const confirmDialogs = [
    {
      open: openComplete,
      onOpenChange: setOpenComplete,
      title: "Complete Order",
      description: `Are you sure you want to mark order #${order.id.slice(0, 8)} as completed? This action cannot be undone.`,
      actionLabel: "Complete",
      actionType: "complete" as const,
      isLoading: completeOrderMutation.isPending,
      onConfirm: async () => {
        completeOrderMutation.mutate(order.id, {
          onSuccess: () => setOpenComplete(false),
        });
      },
    },
    {
      open: openCancel,
      onOpenChange: setOpenCancel,
      title: "Cancel Order",
      description: `Are you sure you want to cancel order #${order.id.slice(0, 8)}? This action cannot be undone.`,
      actionLabel: "Cancel",
      actionType: "cancel" as const,
      isLoading: cancelOrderMutation.isPending,
      onConfirm: async () => {
        cancelOrderMutation.mutate(order.id, {
          onSuccess: () => setOpenCancel(false),
        });
      },
    },
    {
      open: openDelete,
      onOpenChange: setOpenDelete,
      title: "Delete Order",
      description: `Are you sure you want to delete order #${order.id.slice(0, 8)}? This action cannot be undone.`,
      actionLabel: "Delete",
      actionType: "delete" as const,
      isLoading: deleteOrderMutation.isPending,
      onConfirm: async () => {
        deleteOrderMutation.mutate(undefined, {
          onSuccess: () => setOpenDelete(false),
        });
      },
    },
  ];

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
            >
              <CheckCircle2 className="h-4 w-4" />
              {completeOrderMutation.isPending ? "Processing..." : "Complete"}
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="gap-1 bg-transparent text-orange-600 hover:text-orange-700"
              onClick={() => setOpenCancel(true)}
              disabled={cancelOrderMutation.isPending}
            >
              <XCircle className="h-4 w-4" />
              {cancelOrderMutation.isPending ? "Processing..." : "Cancel"}
            </Button>
          </>
        )}
        <Button variant="ghost" size="sm" asChild>
          <Link href={`/orders/${order.id}`}>
            <Eye className="h-4 w-4" />
          </Link>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="text-destructive hover:text-destructive"
          onClick={() => setOpenDelete(true)}
          disabled={deleteOrderMutation.isPending}
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>

      {confirmDialogs.map((dialog) => (
        <ConfirmDialog key={dialog.title} {...dialog} />
      ))}
    </>
  );
}
