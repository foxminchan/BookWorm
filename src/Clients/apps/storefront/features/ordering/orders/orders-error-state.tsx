import { Button } from "@workspace/ui/components/button";
import { Package } from "lucide-react";

export function OrdersErrorState() {
  return (
    <div className="bg-background border border-border p-16 text-center">
      <div className="size-20 bg-secondary rounded-full flex items-center justify-center mx-auto mb-6">
        <Package className="size-10 text-muted-foreground" />
      </div>
      <h2 className="font-serif text-2xl font-medium mb-3">
        Error Loading Orders
      </h2>
      <p className="text-muted-foreground mb-8 max-w-sm mx-auto">
        We encountered an error while loading your orders. Please try again
        later.
      </p>
      <Button onClick={() => window.location.reload()} variant="outline">
        Retry
      </Button>
    </div>
  );
}
