"use client";

import { useState } from "react";

import type { OrderStatus } from "@workspace/types/ordering/orders";

import { PageContainer } from "@/components/page-container";
import { StatusFilter } from "@/features/orders/status-filter";
import { OrdersTable } from "@/features/orders/table/table";

const breadcrumbs = [
  { label: "Admin", href: "/" },
  { label: "Orders", isActive: true },
];

export function OrdersListing() {
  const [statusFilter, setStatusFilter] = useState<OrderStatus | undefined>();

  return (
    <PageContainer
      title="Orders Management"
      description="Manage and track all customer orders"
      breadcrumbs={breadcrumbs}
    >
      <StatusFilter
        statusFilter={statusFilter}
        onStatusChange={setStatusFilter}
      />

      <OrdersTable statusFilter={statusFilter} />
    </PageContainer>
  );
}
