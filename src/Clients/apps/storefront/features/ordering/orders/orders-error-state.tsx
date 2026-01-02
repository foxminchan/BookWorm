import { Package } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

export default function OrdersErrorState() {
  return (
    <div className="bg-background border-border border p-16 text-center">
      <div className="bg-secondary mx-auto mb-6 flex size-20 items-center justify-center rounded-full">
        <Package className="text-muted-foreground size-10" />
      </div>
      <h2 className="mb-3 font-serif text-2xl font-medium">
        Error Loading Orders
      </h2>
      <p className="text-muted-foreground mx-auto mb-8 max-w-sm">
        We encountered an error while loading your orders. Please try again
        later.
      </p>
      <Button onClick={() => window.location.reload()} variant="outline">
        Retry
      </Button>
    </div>
  );
}
