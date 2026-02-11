"use client";

import { useMemo } from "react";

import {
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import type { Order } from "@workspace/types/ordering/orders";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@workspace/ui/components/card";

import { OrdersRevenueChartSkeleton } from "@/components/loading-skeleton";
import { CHART_COLORS, CHART_THEME } from "@/lib/constants";

type OrdersRevenueChartProps = Readonly<{
  orders: Order[];
  isLoading: boolean;
}>;

export function OrdersRevenueChart({
  orders,
  isLoading,
}: OrdersRevenueChartProps) {
  const ordersByDate = useMemo(() => {
    const dateMap = new Map<
      string,
      { date: string; orders: number; revenue: number }
    >();
    for (const order of orders) {
      const date = new Date(order.date).toLocaleDateString();
      const existing = dateMap.get(date);
      if (existing) {
        existing.orders += 1;
        existing.revenue += order.total ?? 0;
      } else {
        dateMap.set(date, { date, orders: 1, revenue: order.total ?? 0 });
      }
    }
    return Array.from(dateMap.values()).slice(-7);
  }, [orders]);

  if (isLoading) {
    return <OrdersRevenueChartSkeleton />;
  }

  return (
    <Card className="lg:col-span-2">
      <CardHeader>
        <CardTitle>Orders & Revenue Trend</CardTitle>
        <CardDescription>Daily orders and revenue</CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={ordersByDate}>
            <CartesianGrid
              strokeDasharray={CHART_THEME.grid.strokeDasharray}
              stroke={CHART_THEME.grid.stroke}
            />
            <XAxis dataKey="date" stroke={CHART_THEME.axis.stroke} />
            <YAxis stroke={CHART_THEME.axis.stroke} />
            <Tooltip contentStyle={CHART_THEME.tooltip} />
            <Legend />
            <Line
              type="monotone"
              dataKey="orders"
              stroke={CHART_COLORS[1]}
              name="Orders"
            />
            <Line
              type="monotone"
              dataKey="revenue"
              stroke={CHART_COLORS[0]}
              name="Revenue ($)"
            />
          </LineChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  );
}
