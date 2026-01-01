import { Package } from "lucide-react";

type OrderItem = {
  id: string;
  name?: string | null;
  quantity: number;
  price: number;
};

type OrderItemsListProps = {
  items: OrderItem[];
};

export default function OrderItemsList({ items }: OrderItemsListProps) {
  return (
    <div className="border border-border/40 rounded-lg overflow-hidden bg-background">
      <div className="border-b border-border/40 px-6 py-4">
        <div className="flex items-center gap-2">
          <Package className="size-5 text-muted-foreground" />
          <h2 className="font-serif text-xl font-semibold">Order Items</h2>
        </div>
      </div>
      <div className="divide-y divide-border/40">
        {items.map((item) => (
          <div
            key={item.id}
            className="px-6 py-4 hover:bg-secondary/20 transition-colors"
          >
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <h3 className="font-medium mb-1">
                  {item.name || "Unnamed Item"}
                </h3>
                <p className="text-sm text-muted-foreground">
                  Quantity: {item.quantity}
                </p>
              </div>
              <div className="text-right">
                <p className="font-semibold">
                  ${(item.price * item.quantity).toFixed(2)}
                </p>
                <p className="text-xs text-muted-foreground">
                  ${item.price.toFixed(2)} each
                </p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
