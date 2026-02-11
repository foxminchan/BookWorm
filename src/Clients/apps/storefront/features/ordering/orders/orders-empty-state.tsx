import { Package } from "lucide-react";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Button } from "@workspace/ui/components/button";

type OrdersEmptyStateProps = {
  selectedStatus: OrderStatus | "All";
  onClearFilter: () => void;
};

export default function OrdersEmptyState({
  selectedStatus,
  onClearFilter,
}: Readonly<OrdersEmptyStateProps>) {
  return (
    <div className="bg-background border-border border p-16 text-center">
      <div className="bg-secondary mx-auto mb-6 flex size-20 items-center justify-center rounded-full">
        <Package className="text-muted-foreground size-10" aria-hidden="true" />
      </div>
      <h2 className="mb-3 font-serif text-2xl font-medium">No Orders Found</h2>
      <p className="text-muted-foreground mx-auto mb-8 max-w-sm">
        We couldn't find any orders matching your selected criteria. Start
        shopping to place your first order.
      </p>
      {selectedStatus !== "All" && (
        <Button
          type="button"
          onClick={onClearFilter}
          variant="outline"
          aria-label="Clear order status filter"
        >
          Clear Filter
        </Button>
      )}
    </div>
  );
}
