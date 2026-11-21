"use client";

import { notFound } from "next/navigation";

import { format } from "date-fns";

import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

import { OrderDetailSkeleton } from "@/components/loading-skeleton";
import { PageContainer } from "@/components/page-container";
import { OrderEventHistory } from "@/features/orders/order-event-history";
import { OrderItemsTable } from "@/features/orders/order-items-table";
import { OrderSummaryCards } from "@/features/orders/order-summary-cards";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Orders", href: "/orders" },
  { label: "Details", isActive: true },
];

type OrderDetailViewProps = Readonly<{
  orderId: string;
}>;

export function OrderDetailView({ orderId }: OrderDetailViewProps) {
  const { data: order, isLoading, error } = useOrder(orderId);

  if (isLoading) {
    return (
      <PageContainer
        title="Order Details"
        description="Loading order details..."
        breadcrumbs={breadcrumbs}
      >
        <OrderDetailSkeleton />
        <OrderEventHistory orderId={orderId} />
      </PageContainer>
    );
  }

  if (error || !order) {
    notFound();
  }

  return (
    <PageContainer
      title={`Order #${order.id.slice(0, 8)}`}
      description={`Order placed on ${format(order.date, "MMM dd, yyyy")}`}
      breadcrumbs={breadcrumbs}
    >
      <OrderSummaryCards
        status={order.status}
        total={order.total}
        itemCount={order.items.length}
      />
      <OrderItemsTable items={order.items} total={order.total} />
      <OrderEventHistory orderId={order.id} />
    </PageContainer>
  );
}
