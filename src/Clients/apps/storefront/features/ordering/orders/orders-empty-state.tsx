import { Button } from "@workspace/ui/components/button";
import { Package } from "lucide-react";
import type { OrderStatus } from "@workspace/types/ordering/orders";

type OrdersEmptyStateProps = {
  selectedStatus: OrderStatus | "All";
  onClearFilter: () => void;
};

export default function OrdersEmptyState({
  selectedStatus,
  onClearFilter,
}: OrdersEmptyStateProps) {
  return (
    <div className="bg-background border border-border p-16 text-center">
      <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto mb-6">
        <Package className="size-10 text-muted-foreground" />
      </div>
      <h2 className="font-serif text-2xl font-medium mb-3">No Orders Found</h2>
      <p className="text-muted-foreground mb-8 max-w-sm mx-auto">
        We couldn't find any orders matching your selected criteria. Start
        shopping to place your first order.
      </p>
      {selectedStatus !== "All" && (
        <Button onClick={onClearFilter} variant="outline">
          Clear Filter
        </Button>
      )}
    </div>
  );
}
