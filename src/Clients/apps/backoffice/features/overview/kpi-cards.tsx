"use client";

import { useMemo } from "react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from "@workspace/ui/components/card";

import { KPICardsSkeleton } from "@/components/loading-skeleton";
import { currencyFormatter } from "@/lib/constants";

type KPICardsProps = Readonly<{
  totalOrders: number;
  totalRevenue: number;
  totalCustomers: number;
  totalBooks: number;
  isLoading: boolean;
}>;

export function KPICards({
  totalOrders,
  totalRevenue,
  totalCustomers,
  totalBooks,
  isLoading,
}: KPICardsProps) {
  const kpiData = useMemo(() => {
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
        title: "Registered Customers",
        value: totalCustomers.toString(),
        change: "Total registered",
      },
      {
        title: "Books in Catalog",
        value: totalBooks.toString(),
        change: "Available titles",
      },
    ];
  }, [totalRevenue, totalOrders, totalCustomers, totalBooks]);

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
