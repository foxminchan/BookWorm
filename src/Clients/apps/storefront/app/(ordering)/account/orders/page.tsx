"use client";

import { useState } from "react";

import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import type { OrderStatus } from "@workspace/types/ordering/orders";

import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import { OrdersSkeleton } from "@/components/loading-skeleton";
import { Pagination } from "@/components/pagination";
import OrdersEmptyState from "@/features/ordering/orders/orders-empty-state";
import OrdersErrorState from "@/features/ordering/orders/orders-error-state";
import OrdersHeader from "@/features/ordering/orders/orders-header";
import OrdersList from "@/features/ordering/orders/orders-list";

const ORDERS_PER_PAGE = 5;

export default function OrdersPage() {
  const [selectedStatus, setSelectedStatus] = useState<OrderStatus | "All">(
    "All",
  );
  const [currentPage, setCurrentPage] = useState(1);

  const {
    data: ordersData,
    isLoading,
    error,
  } = useOrders({
    pageIndex: currentPage - 1,
    pageSize: ORDERS_PER_PAGE,
    status: selectedStatus === "All" ? undefined : selectedStatus,
  });

  const orders = ordersData?.items ?? [];
  const totalCount = ordersData?.totalCount ?? 0;
  const totalPages = Math.ceil(totalCount / ORDERS_PER_PAGE);

  const handleStatusChange = (value: OrderStatus | "All") => {
    setSelectedStatus(value);
    setCurrentPage(1);
  };

  return (
    <div className="bg-background flex min-h-screen flex-col">
      <Header />

      <main className="container mx-auto max-w-5xl flex-1 px-4 py-16">
        <OrdersHeader
          selectedStatus={selectedStatus}
          onStatusChange={handleStatusChange}
        />

        {isLoading ? (
          <OrdersSkeleton />
        ) : error ? (
          <OrdersErrorState />
        ) : orders.length === 0 ? (
          <OrdersEmptyState
            selectedStatus={selectedStatus}
            onClearFilter={() => setSelectedStatus("All")}
          />
        ) : (
          <>
            <OrdersList orders={orders} />

            {totalPages > 1 && (
              <div className="border-border mt-16 border-t pt-8">
                <Pagination
                  currentPage={currentPage}
                  totalPages={totalPages}
                  onPageChange={setCurrentPage}
                />
              </div>
            )}
          </>
        )}
      </main>

      <Footer />
    </div>
  );
}
