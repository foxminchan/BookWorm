"use client";

import { useMemo } from "react";

import type { Order } from "@workspace/types/ordering/orders";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from "@workspace/ui/components/card";

import { KPICardsSkeleton } from "@/components/loading-skeleton";
import { currencyFormatter } from "@/lib/constants";

type KPICardsProps = Readonly<{
  orders: Order[];
  totalCustomers: number;
  totalBooks: number;
  isLoading: boolean;
}>;

export function KPICards({
  orders,
  totalCustomers,
  totalBooks,
  isLoading,
}: KPICardsProps) {
  const kpiData = useMemo(() => {
    const totalRevenue = orders.reduce(
      (sum, order) => sum + (order.total ?? 0),
      0,
    );
    const totalOrders = orders.length;

    return [
      {
        title: "Total Revenue",
        value: currencyFormatter.format(totalRevenue),
        change: `${totalOrders} orders`,
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
  }, [orders, totalCustomers, totalBooks]);

  if (isLoading) {
    return <KPICardsSkeleton />;
  }

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

type KPICardProps = Readonly<{
  title: string;
  value: string;
  change: string;
}>;

function KPICard({ title, value, change }: KPICardProps) {
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
