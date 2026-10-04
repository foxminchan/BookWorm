"use client";

import dynamic from "next/dynamic";

import { Button } from "@workspace/ui/components/button";

import { KPICards } from "@/features/overview/kpi-cards";
import { RecentOrdersTable } from "@/features/overview/recent-orders-table";
import { useDashboardStats } from "@/hooks/useDashboardStats";

// Dynamic imports for heavy recharts-based components (~300KB)
const OrdersRevenueChart = dynamic(
  () =>
    import("@/features/overview/orders-revenue-chart").then(
      (m) => m.OrdersRevenueChart,
    ),
  { ssr: false },
);

const BooksCategoryChart = dynamic(
  () =>
    import("@/features/overview/books-category-chart").then(
      (m) => m.BooksCategoryChart,
    ),
  { ssr: false },
);

export default function OverviewTab() {
  const { data, isLoading, error, isDisconnected, refetch } =
    useDashboardStats();

  if (error && !data) {
    return (
      <div role="alert" className="space-y-4">
        <p>Unable to load dashboard statistics.</p>
        <Button onClick={() => void refetch()}>Retry</Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {isDisconnected && (
        <output className="text-muted-foreground text-sm">
          Live updates are reconnecting. Showing the last available statistics.
        </output>
      )}
      <KPICards
        totalOrders={data?.totalOrders ?? 0}
        totalRevenue={data?.totalRevenue ?? 0}
        totalCustomers={data?.totalCustomers ?? 0}
        totalBooks={data?.totalBooks ?? 0}
        isLoading={isLoading}
      />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <OrdersRevenueChart
          dailyOrders={data?.dailyOrders ?? []}
          isLoading={isLoading}
        />
        <BooksCategoryChart
          categories={data?.categories ?? []}
          isLoading={isLoading}
        />
      </div>

      <RecentOrdersTable
        orders={data?.recentOrders ?? []}
        isLoading={isLoading}
      />
    </div>
  );
}
