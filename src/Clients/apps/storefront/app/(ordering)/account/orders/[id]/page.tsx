"use client";

import { use } from "react";
import { Header } from "@/components/header";
import { Footer } from "@/components/footer";
import { EmptyState } from "@/components/empty-state";
import { OrderDetailSkeleton } from "@/components/loading-skeleton";
import {
  OrderDetailHeader,
  OrderItemsList,
  OrderSummary,
} from "@/features/ordering/order-detail";
import { PackageX } from "lucide-react";
import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

export default function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: orderDetail, isLoading, error } = useOrder(id);

  if (isLoading) {
    return (
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
          <OrderDetailSkeleton />
        </main>
        <Footer />
      </div>
    );
  }

  if (error || !orderDetail) {
    return (
      <div className="min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
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
    <div className="min-h-screen flex flex-col">
      <Header />

      <main className="flex-1 container mx-auto px-4 py-12 max-w-4xl">
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
