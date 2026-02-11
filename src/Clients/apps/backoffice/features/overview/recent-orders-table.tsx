"use client";

import { useMemo } from "react";

import {
  type CellContext,
  type ColumnDef,
  flexRender,
  getCoreRowModel,
  useReactTable,
} from "@tanstack/react-table";
import { format } from "date-fns";

import type { Order } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@workspace/ui/components/table";

import { RecentOrdersTableSkeleton } from "@/components/loading-skeleton";
import { currencyFormatter } from "@/lib/constants";
import { type OrderStatus, getOrderStatusStyle } from "@/lib/pattern";

type RecentOrdersTableProps = Readonly<{
  orders: Order[];
  isLoading: boolean;
}>;

function OrderIdCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <div className="font-medium">#{row.original.id.slice(0, 8)}</div>;
}

function AmountCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return <div>{currencyFormatter.format(row.original.total ?? 0)}</div>;
}

function StatusCell({ row }: Readonly<CellContext<Order, unknown>>) {
  const status = row.original.status as OrderStatus;
  return <Badge className={getOrderStatusStyle(status)}>{status}</Badge>;
}

function DateCell({ row }: Readonly<CellContext<Order, unknown>>) {
  return (
    <div className="text-muted-foreground">
      {format(new Date(row.original.date), "MMM dd, yyyy")}
    </div>
  );
}

const columns: ColumnDef<Order>[] = [
  { accessorKey: "id", header: "Order ID", cell: OrderIdCell },
  { accessorKey: "total", header: "Amount", cell: AmountCell },
  { accessorKey: "status", header: "Status", cell: StatusCell },
  { accessorKey: "date", header: "Date", cell: DateCell },
];

export function RecentOrdersTable({
  orders,
  isLoading,
}: RecentOrdersTableProps) {
  const recentOrders = useMemo(() => orders.slice(-5), [orders]);

  const table = useReactTable({
    data: recentOrders,
    columns,
    getCoreRowModel: getCoreRowModel(),
  });

  if (isLoading) {
    return <RecentOrdersTableSkeleton />;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Recent Orders</CardTitle>
        <CardDescription>Last 5 orders placed</CardDescription>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id} scope="col">
                    {header.isPlaceholder
                      ? null
                      : flexRender(
                          header.column.columnDef.header,
                          header.getContext(),
                        )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && "selected"}
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(
                        cell.column.columnDef.cell,
                        cell.getContext(),
                      )}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell
                  colSpan={columns.length}
                  className="h-24 text-center"
                >
                  No orders found.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  );
}
