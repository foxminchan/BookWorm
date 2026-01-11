"use client";

import type { Order } from "@workspace/types/ordering/orders";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from "@workspace/ui/components/card";

import { KPICardsSkeleton } from "@/components/loading-skeleton";

type KPICardsProps = {
  orders: Order[];
  totalCustomers: number;
  totalBooks: number;
  isLoading: boolean;
};

export function KPICards({
  orders,
  totalCustomers,
  totalBooks,
  isLoading,
}: KPICardsProps) {
  const formatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  });
  if (isLoading) {
    return <KPICardsSkeleton />;
  }

  const totalRevenue = orders.reduce(
    (sum: number, order: Order) => sum + (order.total || 0),
    0,
  );
  const totalOrders = orders.length;

  const kpiData = [
    {
      title: "Total Revenue",
      value: formatter.format(totalRevenue),
      change: `${orders.length} orders`,
    },
    {
      title: "Total Orders",
      value: totalOrders.toString(),
      change: `${totalCustomers} customers`,
    },
    {
      title: "Active Customers",
      value: totalCustomers.toString(),
      change: "Total registered",
    },
    {
      title: "Books in Catalog",
      value: totalBooks.toString(),
      change: "Available titles",
    },
  ];

  return (
    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
      {kpiData.map((kpi) => (
        <KPICard
          key={kpi.title}
          title={kpi.title}
          value={kpi.value}
          change={kpi.change}
        />
      ))}
    </div>
  );
}

function KPICard({
  title,
  value,
  change,
}: {
  title: string;
  value: string;
  change: string;
}) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardDescription>{title}</CardDescription>
      </CardHeader>
      <CardContent>
        <div className="text-foreground text-2xl font-bold">{value}</div>
        <p className="text-primary mt-2 text-xs">{change}</p>
      </CardContent>
    </Card>
  );
}
