"use client";

import { use } from "react";

import { PackageX } from "lucide-react";

import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

import { EmptyState } from "@/components/empty-state";
import { Footer } from "@/components/footer";
import { Header } from "@/components/header";
import { OrderDetailSkeleton } from "@/components/loading-skeleton";
import OrderDetailHeader from "@/features/ordering/order-detail/order-detail-header";
import OrderItemsList from "@/features/ordering/order-detail/order-items-list";
import OrderSummary from "@/features/ordering/order-detail/order-summary";

export default function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: orderDetail, isLoading, error } = useOrder(id);

  if (isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
          <OrderDetailSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (error || !orderDetail) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
          <EmptyState
            icon={PackageX}
            title="Order Not Found"
            description="We couldn't find the order you're looking for."
            actionLabel="Back to Orders"
            actionHref="/account/orders"
          />
        </main>
        <Footer />
      </div>
    );
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />

      <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
        <OrderDetailHeader
          orderId={orderDetail.id}
          status={orderDetail.status}
          date={orderDetail.date}
        />

        <div className="grid gap-6">
          <OrderItemsList items={orderDetail.items} />
          <OrderSummary total={orderDetail.total} />
        </div>
      </main>

      <Footer />
    </div>
  );
}
