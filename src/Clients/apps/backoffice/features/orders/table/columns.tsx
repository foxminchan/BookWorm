"use client";

import type { ColumnDef } from "@tanstack/react-table";
import { format } from "date-fns";

import type { Order } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";

import { type OrderStatus, getOrderStatusStyle } from "@/lib/pattern";

import { CellAction } from "./cell-action";

export const columns: ColumnDef<Order>[] = [
  {
    accessorKey: "id",
    header: "Order ID",
    cell: ({ row }) => (
      <div className="font-medium">#{row.original.id.slice(0, 8)}</div>
    ),
  },
  {
    accessorKey: "date",
    header: "Date",
    cell: ({ row }) => (
      <div>{format(new Date(row.original.date), "MMM dd, yyyy")}</div>
    ),
  },
  {
    accessorKey: "total",
    header: "Total",
    cell: ({ row }) => {
      const formatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      });
      return (
        <div className="font-medium">
          {formatter.format(row.original.total)}
        </div>
      );
    },
  },
  {
    accessorKey: "status",
    header: "Status",
    cell: ({ row }) => {
      const status = row.original.status as OrderStatus;
      return <Badge className={getOrderStatusStyle(status)}>{status}</Badge>;
    },
  },
  {
    id: "actions",
    cell: ({ row }) => <CellAction order={row.original} />,
  },
];
