"use client";

import type { CellContext, ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";

import type { Order } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { formatPrice } from "@workspace/utils/format";

import { type OrderStatus, getOrderStatusStyle } from "@/lib/pattern";
import { backofficeTableFeatures } from "@/lib/table";

import { CellAction } from "./cell-action";

type OrderCellContext = CellContext<
  typeof backofficeTableFeatures,
  Order,
  unknown
>;

function OrderIdCell({ row }: Readonly<OrderCellContext>) {
  return <div className="font-medium">#{row.original.id.slice(0, 8)}</div>;
}

function DateCell({ row }: Readonly<OrderCellContext>) {
  return <div>{format(new Date(row.original.date), "MMM dd, yyyy")}</div>;
}

function TotalCell({ row }: Readonly<OrderCellContext>) {
  return <div className="font-medium">{formatPrice(row.original.total)}</div>;
}

function StatusCell({ row }: Readonly<OrderCellContext>) {
  const status = row.original.status as OrderStatus;
  return <Badge className={getOrderStatusStyle(status)}>{status}</Badge>;
}

function ActionsCell({ row }: Readonly<OrderCellContext>) {
  return <CellAction order={row.original} />;
}

export const columns: ColumnDef<typeof backofficeTableFeatures, Order>[] = [
  { accessorKey: "id", header: "Order ID", cell: OrderIdCell },
  { accessorKey: "date", header: "Date", cell: DateCell },
  { accessorKey: "total", header: "Total", cell: TotalCell },
  { accessorKey: "status", header: "Status", cell: StatusCell },
  { id: "actions", cell: ActionsCell },
];
