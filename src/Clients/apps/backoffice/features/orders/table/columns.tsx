"use client";

import type { CellContext, ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";

import type { Order } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { formatPrice } from "@workspace/utils/format";

import { type OrderStatus, getOrderStatusStyle } from "@/lib/pattern";

import { CellAction } from "./cell-action";

function OrderIdCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <div className="font-medium">#{row.original.id.slice(0, 8)}</div>;
}

function DateCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <div>{format(new Date(row.original.date), "MMM dd, yyyy")}</div>;
}

function TotalCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <div className="font-medium">{formatPrice(row.original.total)}</div>;
}

function StatusCell({ row }: Readonly<CellContext<Order, unknown>>) {
  const status = row.original.status as OrderStatus;
  return <Badge className={getOrderStatusStyle(status)}>{status}</Badge>;
}

function ActionsCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <CellAction order={row.original} />;
}

export const columns: ColumnDef<Order>[] = [
  { accessorKey: "id", header: "Order ID", cell: OrderIdCell },
  { accessorKey: "date", header: "Date", cell: DateCell },
  { accessorKey: "total", header: "Total", cell: TotalCell },
  { accessorKey: "status", header: "Status", cell: StatusCell },
  { id: "actions", cell: ActionsCell },
];
