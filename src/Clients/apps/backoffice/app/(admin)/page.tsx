"use client";

import { BooksCategoryChart } from "@/features/overview/books-category-chart";
import { KPICards } from "@/features/overview/kpi-cards";
import { OrdersRevenueChart } from "@/features/overview/orders-revenue-chart";
import { RecentOrdersTable } from "@/features/overview/recent-orders-table";
import { useDashboardStats } from "@/hooks/use-dashboard-stats";

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
