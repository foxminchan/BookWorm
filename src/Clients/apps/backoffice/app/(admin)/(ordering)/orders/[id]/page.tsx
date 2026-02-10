"use client";

import { use } from "react";

import { notFound } from "next/navigation";

import { format } from "date-fns";

import useOrder from "@workspace/api-hooks/ordering/orders/useOrder";

import { OrderDetailSkeleton } from "@/components/loading-skeleton";
import { PageHeader } from "@/components/page-header";
import { OrderItemsTable } from "@/features/orders/order-items-table";
import { OrderSummaryCards } from "@/features/orders/order-summary-cards";

type OrderDetailPageProps = {
  params: Promise<{ id: string }>;
};

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Orders", href: "/orders" },
  { label: "Details", isActive: true },
];

export default function OrderDetailPage({
  params,
}: Readonly<OrderDetailPageProps>) {
  const { id } = use(params);
  const { data: order, isLoading, error } = useOrder(id);

  if (error) {
    notFound();
  }

  if (isLoading) {
    return (
      <div className="space-y-6">
        <PageHeader
          title="Order Details"
          description="Loading order details..."
          breadcrumbs={breadcrumbs}
        />
        <OrderDetailSkeleton />
      </div>
    );
  }

  if (!order) {
    notFound();
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title={`Order #${order.id.slice(0, 8)}`}
        description={`Order placed on ${format(new Date(order.date), "MMM dd, yyyy")}`}
        breadcrumbs={breadcrumbs}
      />
      <OrderSummaryCards
        status={order.status}
        total={order.total}
        itemCount={order.items.length}
      />
      <OrderItemsTable items={order.items} total={order.total} />
    </div>
  );
}
