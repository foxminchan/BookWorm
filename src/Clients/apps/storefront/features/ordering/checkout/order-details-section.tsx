import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { formatPrice } from "@workspace/utils/format";

import { getOrderStatusColor } from "@/lib/pattern";

type OrderDetailsSectionProps = {
  status: OrderStatus;
  total: number;
  buyerName?: string;
  buyerAddress?: string;
};

export default function OrderDetailsSection({
  status,
  total,
  buyerName,
  buyerAddress,
}: Readonly<OrderDetailsSectionProps>) {
  return (
    <div className="border-border mb-24 grid gap-8 border-y py-12 md:grid-cols-3">
      {/* Shipping Address */}
      <div className="space-y-4">
        <h3 className="text-foreground font-serif text-lg font-medium tracking-wide uppercase">
          Shipping Address
        </h3>
        <div className="text-muted-foreground space-y-1 leading-relaxed">
          {buyerName ? (
            <p className="text-foreground font-medium">{buyerName}</p>
          ) : null}
          {buyerAddress ? (
            <p>{buyerAddress}</p>
          ) : (
            <p className="text-muted-foreground italic">No address set</p>
          )}
        </div>
      </div>

      {/* Order Status */}
      <div className="space-y-4">
        <h3 className="text-foreground font-serif text-lg font-medium tracking-wide uppercase">
          Order Status
        </h3>
        <div className="flex flex-col items-start">
          <p className="text-muted-foreground mb-2 text-sm">Current Status</p>
          <Badge
            className={`${getOrderStatusColor(status)} border-0 px-3 py-1 text-base`}
          >
            {status}
          </Badge>
        </div>
      </div>

      {/* Order Total */}
      <div className="space-y-4">
        <h3 className="text-foreground font-serif text-lg font-medium tracking-wide uppercase">
          Order Total
        </h3>
        <div className="flex flex-col items-start">
          <p className="text-muted-foreground mb-2 text-sm">Amount Paid</p>
          <p className="text-primary font-serif text-4xl font-medium">
            {formatPrice(total)}
          </p>
        </div>
      </div>
    </div>
  );
}
