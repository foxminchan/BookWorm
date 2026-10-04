"use client";

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

import type { DailyOrders } from "@workspace/types/ordering/dashboard";
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
  dailyOrders: DailyOrders[];
  isLoading: boolean;
}>;

export function OrdersRevenueChart({
  dailyOrders,
  isLoading,
}: OrdersRevenueChartProps) {
  if (isLoading) {
    return <OrdersRevenueChartSkeleton />;
  }

  return (
    <Card className="lg:col-span-2">
      <CardHeader>
        <CardTitle>Orders & Revenue Trend</CardTitle>
        <CardDescription>
          Daily orders and completed-order revenue (UTC)
        </CardDescription>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={dailyOrders}>
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
