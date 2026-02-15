"use client";

import dynamic from "next/dynamic";

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
  const { books, orders, customers, isLoading } = useDashboardStats();

  return (
    <div className="space-y-6">
      <KPICards
        orders={orders}
        totalCustomers={customers.length}
        totalBooks={books.length}
        isLoading={isLoading}
      />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <OrdersRevenueChart orders={orders} isLoading={isLoading} />
        <BooksCategoryChart books={books} isLoading={isLoading} />
      </div>

      <RecentOrdersTable orders={orders} isLoading={isLoading} />
    </div>
  );
}
