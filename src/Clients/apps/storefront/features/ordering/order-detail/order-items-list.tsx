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
  const formatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  });
  return (
    <div className="border-border/40 bg-background overflow-hidden rounded-lg border">
      <div className="border-border/40 border-b px-6 py-4">
        <div className="flex items-center gap-2">
          <Package className="text-muted-foreground size-5" />
          <h2 className="font-serif text-xl font-semibold">Order Items</h2>
        </div>
      </div>
      <div className="divide-border/40 divide-y">
        {items.map((item) => (
          <div
            key={item.id}
            className="hover:bg-secondary/20 px-6 py-4 transition-colors"
          >
            <div className="flex items-center justify-between">
              <div className="flex-1">
                <h3 className="mb-1 font-medium">
                  {item.name || "Unnamed Item"}
                </h3>
                <p className="text-muted-foreground text-sm">
                  Quantity: {item.quantity}
                </p>
              </div>
              <div className="text-right">
                <p className="font-semibold">
                  {formatter.format(item.price * item.quantity)}
                </p>
                <p className="text-muted-foreground text-xs">
                  {formatter.format(item.price)} each
                </p>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
