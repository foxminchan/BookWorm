"use client";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Button } from "@workspace/ui/components/button";

type StatusFilterProps = {
  statusFilter: OrderStatus | undefined;
  onStatusChange: (status: OrderStatus | undefined) => void;
};

export function StatusFilter({
  statusFilter,
  onStatusChange,
}: StatusFilterProps) {
  const statuses: Array<{ value: OrderStatus | undefined; label: string }> = [
    { value: undefined, label: "All Orders" },
    { value: "New", label: "New" },
    { value: "Completed", label: "Completed" },
    { value: "Cancelled", label: "Cancelled" },
  ];

  return (
    <div className="flex gap-2 overflow-x-auto pb-2">
      {statuses.map((status) => (
        <Button
          key={status.value ?? "all"}
          variant={statusFilter === status.value ? "default" : "outline"}
          onClick={() => onStatusChange(status.value)}
          className="whitespace-nowrap"
        >
          {status.label}
        </Button>
      ))}
    </div>
  );
}
