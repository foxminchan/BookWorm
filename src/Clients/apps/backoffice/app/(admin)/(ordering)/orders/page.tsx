"use client";

import { useState } from "react";

import type { OrderStatus } from "@workspace/types/ordering/orders";

import { PageHeader } from "@/components/page-header";
import { StatusFilter } from "@/features/orders/status-filter";
import { OrdersTable } from "@/features/orders/table/table";

export default function OrdersPage() {
  const [statusFilter, setStatusFilter] = useState<OrderStatus | undefined>(
    undefined,
  );

  return (
    <div className="space-y-6">
      <PageHeader
        title="Orders Management"
        description="Manage and track all customer orders"
        breadcrumbs={[
          { label: "Admin", href: "/" },
          { label: "Orders", isActive: true },
        ]}
      />

      <StatusFilter
        statusFilter={statusFilter}
        onStatusChange={setStatusFilter}
      />

      <OrdersTable statusFilter={statusFilter} />
    </div>
  );
}
