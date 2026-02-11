"use client";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Button } from "@workspace/ui/components/button";

type StatusFilterProps = Readonly<{
  statusFilter: OrderStatus | undefined;
  onStatusChange: (status: OrderStatus | undefined) => void;
}>;

const statuses: ReadonlyArray<{
  value: OrderStatus | undefined;
  label: string;
}> = [
  { value: undefined, label: "All Orders" },
  { value: "New", label: "New" },
  { value: "Completed", label: "Completed" },
  { value: "Cancelled", label: "Cancelled" },
];

export function StatusFilter({
  statusFilter,
  onStatusChange,
}: StatusFilterProps) {
  return (
    <fieldset className="flex gap-2 overflow-x-auto border-none p-0 pb-2">
      <legend className="sr-only">Filter orders by status</legend>
      {statuses.map((status) => (
        <Button
          key={status.value ?? "all"}
          variant={statusFilter === status.value ? "default" : "outline"}
          onClick={() => onStatusChange(status.value)}
          className="whitespace-nowrap"
          aria-pressed={statusFilter === status.value}
        >
          {status.label}
        </Button>
      ))}
    </fieldset>
  );
}
