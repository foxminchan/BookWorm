"use client";

import type { Order } from "@workspace/types/ordering/orders";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { RecentOrdersTableSkeleton } from "@/components/loading-skeleton";

type RecentOrdersTableProps = {
  orders: Order[];
  isLoading: boolean;
};

export function RecentOrdersTable({
  orders,
  isLoading,
}: RecentOrdersTableProps) {
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
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-border border-b">
                <th className="text-muted-foreground px-4 py-2 text-left font-medium">
                  Order ID
                </th>
                <th className="text-muted-foreground px-4 py-2 text-left font-medium">
                  Amount
                </th>
                <th className="text-muted-foreground px-4 py-2 text-left font-medium">
                  Status
                </th>
                <th className="text-muted-foreground px-4 py-2 text-left font-medium">
                  Date
                </th>
              </tr>
            </thead>
            <tbody>
              {orders.slice(-5).map((order: Order) => (
                <tr key={order.id} className="border-border border-b">
                  <td className="text-foreground px-4 py-2">
                    #ORD-{order.id.slice(0, 8)}
                  </td>
                  <td className="text-foreground px-4 py-2">
                    ${order.total?.toFixed(2) || "0.00"}
                  </td>
                  <td className="px-4 py-2">
                    <span className="bg-primary/20 text-primary rounded-full px-2 py-1 text-xs">
                      {order.status}
                    </span>
                  </td>
                  <td className="text-muted-foreground px-4 py-2">
                    {new Date(order.date).toLocaleDateString()}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  );
}
