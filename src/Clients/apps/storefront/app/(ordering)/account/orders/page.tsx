"use client";

import { useState } from "react";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { Package, ChevronRight, ArrowLeft } from "lucide-react";
import Link from "next/link";
import { Pagination } from "@/components/pagination";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";
import { Skeleton } from "@workspace/ui/components/skeleton";
import useOrders from "@workspace/api-hooks/ordering/orders/useOrders";
import { getOrderStatusColorBordered } from "@/lib/pattern";

const ORDERS_PER_PAGE = 5;
const STATUS_OPTIONS: (OrderStatus | "All")[] = [
  "All",
  "New",
  "Completed",
  "Cancelled",
];

function OrdersSkeleton() {
  return (
    <>
      <div className="space-y-px border border-border/40 bg-border/40">
        {Array.from({ length: 5 }).map((_, i) => (
          <div
            key={i}
            className="bg-background p-8 border-b border-border/40 last:border-b-0"
          >
            <div className="space-y-4">
              <div className="flex items-center gap-4">
                <Skeleton className="h-8 w-32" />
                <Skeleton className="h-6 w-20" />
              </div>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-6 pt-6 border-t border-border">
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
                <Skeleton className="h-12 w-full" />
              </div>
            </div>
          </div>
        ))}
      </div>
    </>
  );
}

export default function OrdersPage() {
  const [selectedStatus, setSelectedStatus] = useState<OrderStatus | "All">(
    "All",
  );
  const [currentPage, setCurrentPage] = useState(1);

  // API hook with windowed pagination
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
    <div className="min-h-screen flex flex-col bg-background">
      <Header />

      <main className="flex-1 container mx-auto px-4 py-16 max-w-5xl">
        <div className="mb-12 flex flex-col md:flex-row md:items-end justify-between gap-8">
          <div className="flex-1">
            <Link href="/account">
              <Button
                variant="ghost"
                className="mb-6 gap-2 -ml-2 text-muted-foreground hover:text-foreground"
              >
                <ArrowLeft className="size-4" />
                Back to Account
              </Button>
            </Link>
            <div className="space-y-2">
              <h1 className="font-serif text-5xl font-medium text-balance">
                Order History
              </h1>
              <p className="text-lg text-muted-foreground">
                Track and manage all your purchases
              </p>
            </div>
          </div>

          <div className="w-full md:w-56">
            <label className="text-xs font-semibold uppercase tracking-widest text-muted-foreground mb-3 block">
              Filter Orders
            </label>
            <Select value={selectedStatus} onValueChange={handleStatusChange}>
              <SelectTrigger className="border-2 hover:border-primary/50 transition-colors">
                <SelectValue placeholder="Select status" />
              </SelectTrigger>
              <SelectContent>
                {STATUS_OPTIONS.map((status) => (
                  <SelectItem key={status} value={status}>
                    {status}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        {isLoading ? (
          <OrdersSkeleton />
        ) : error ? (
          <div className="bg-background border border-border p-16 text-center">
            <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto mb-6">
              <Package className="size-10 text-muted-foreground" />
            </div>
            <h2 className="font-serif text-2xl font-medium mb-3">
              Error Loading Orders
            </h2>
            <p className="text-muted-foreground mb-8 max-w-sm mx-auto">
              We encountered an error while loading your orders. Please try
              again later.
            </p>
            <Button onClick={() => window.location.reload()} variant="outline">
              Retry
            </Button>
          </div>
        ) : orders.length === 0 ? (
          <div className="bg-background border border-border p-16 text-center">
            <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto mb-6">
              <Package className="size-10 text-muted-foreground" />
            </div>
            <h2 className="font-serif text-2xl font-medium mb-3">
              No Orders Found
            </h2>
            <p className="text-muted-foreground mb-8 max-w-sm mx-auto">
              We couldn't find any orders matching your selected criteria. Start
              shopping to place your first order.
            </p>
            {selectedStatus !== "All" && (
              <Button
                onClick={() => setSelectedStatus("All")}
                variant="outline"
              >
                Clear Filter
              </Button>
            )}
          </div>
        ) : (
          <>
            <div className="space-y-px border border-border/40 bg-border/40">
              {orders.map((order: any) => (
                <Link key={order.id} href={`/account/orders/${order.id}`}>
                  <div className="group bg-background hover:bg-secondary/20 transition-all duration-300 p-8 cursor-pointer border-b border-border/40 last:border-b-0">
                    <div className="flex items-start justify-between gap-6">
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center gap-4 mb-6">
                          <div>
                            <p className="text-xs uppercase tracking-widest text-muted-foreground mb-1">
                              Order ID
                            </p>
                            <h3 className="font-serif text-2xl font-medium">
                              {order.id}
                            </h3>
                          </div>
                          <Badge
                            className={`${getOrderStatusColorBordered(order.status)} px-3 py-1 text-xs font-semibold`}
                          >
                            {order.status}
                          </Badge>
                        </div>

                        <div className="grid grid-cols-2 md:grid-cols-3 gap-6 pt-6 border-t border-border">
                          <div>
                            <p className="text-xs uppercase tracking-widest text-muted-foreground mb-2">
                              Order Date
                            </p>
                            <p className="font-medium">
                              {new Date(order.date).toLocaleDateString(
                                "en-US",
                                {
                                  year: "numeric",
                                  month: "long",
                                  day: "numeric",
                                },
                              )}
                            </p>
                          </div>
                          <div>
                            <p className="text-xs uppercase tracking-widest text-muted-foreground mb-2">
                              Total Amount
                            </p>
                            <p className="font-serif text-lg font-medium">
                              ${order.total.toFixed(2)}
                            </p>
                          </div>
                          <div className="col-span-2 md:col-span-1 flex items-end">
                            <Button
                              variant="ghost"
                              size="sm"
                              className="gap-2 text-primary hover:text-primary/80 font-medium -ml-2 group-hover:translate-x-1 transition-transform"
                            >
                              View Details
                              <ChevronRight className="size-4" />
                            </Button>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </Link>
              ))}
            </div>

            {totalPages > 1 && (
              <div className="mt-16 pt-8 border-t border-border">
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
