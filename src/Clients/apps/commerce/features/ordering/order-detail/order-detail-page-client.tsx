"use client";

import { PackageX } from "lucide-react";

import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";
import { EmptyState } from "@workspace/storefront-shell/components/empty-state";

import { OrderDetailSkeleton } from "@/components/loading-skeleton";
import OrderDetailHeader from "@/features/ordering/order-detail/order-detail-header";
import OrderItemsList from "@/features/ordering/order-detail/order-items-list";
import OrderSummary from "@/features/ordering/order-detail/order-summary";
import OrderTimeline from "@/features/ordering/order-detail/order-timeline";

type OrderDetailPageClientProps = Readonly<{
  orderId: string;
}>;

export default function OrderDetailPageClient({
  orderId,
}: OrderDetailPageClientProps) {
  const { data: orderDetail, isLoading, error } = useOrder(orderId);

  if (isLoading) {
    return (
      <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
        <OrderDetailSkeleton />
      </main>
    );
  }

  if (error || !orderDetail) {
    return (
      <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
        <EmptyState
          icon={PackageX}
          title="Order Not Found"
          description="We couldn't find the order you're looking for."
          actionLabel="Back to Orders"
          actionHref="/account/orders"
        />
      </main>
    );
  }

  return (
    <main className="container mx-auto max-w-4xl flex-1 px-4 py-12">
      <OrderDetailHeader
        orderId={orderDetail.id}
        status={orderDetail.status}
        date={orderDetail.date}
      />

      <div className="grid gap-6">
        <OrderItemsList items={orderDetail.items} />
        <OrderTimeline orderId={orderDetail.id} />
        <OrderSummary total={orderDetail.total} />
      </div>
    </main>
  );
}
