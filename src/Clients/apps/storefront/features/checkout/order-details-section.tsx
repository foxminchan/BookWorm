import { Badge } from "@workspace/ui/components/badge";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import { getOrderStatusColor } from "@/lib/pattern";

type OrderDetailsSectionProps = {
  status: OrderStatus;
  total: number;
};

export function OrderDetailsSection({
  status,
  total,
}: OrderDetailsSectionProps) {
  return (
    <div className="grid md:grid-cols-3 gap-8 mb-24 py-12 border-y border-border">
      {/* Shipping Address */}
      <div className="space-y-4">
        <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
          Shipping Address
        </h3>
        <div className="space-y-1 text-muted-foreground leading-relaxed">
          <p className="font-medium text-foreground">Jane Doe</p>
          <p>123 Literary Lane, Apt 4B</p>
          <p>Booktown, NY 10001</p>
        </div>
      </div>

      {/* Order Status */}
      <div className="space-y-4">
        <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
          Order Status
        </h3>
        <div className="flex flex-col items-start">
          <p className="text-sm text-muted-foreground mb-2">Current Status</p>
          <Badge
            className={`${getOrderStatusColor(status)} border-0 text-base px-3 py-1`}
          >
            {status}
          </Badge>
        </div>
      </div>

      {/* Order Total */}
      <div className="space-y-4">
        <h3 className="font-serif text-lg font-medium text-foreground uppercase tracking-wide">
          Order Total
        </h3>
        <div className="flex flex-col items-start">
          <p className="text-sm text-muted-foreground mb-2">Amount Paid</p>
          <p className="font-serif text-4xl font-medium text-primary">
            ${total.toFixed(2)}
          </p>
        </div>
      </div>
    </div>
  );
}
