import { formatPrice } from "@workspace/utils/format";

type OrderSummaryProps = {
  readonly total: number;
};

export default function OrderSummary({ total }: OrderSummaryProps) {
  return (
    <div className="border-border/40 bg-background hover:bg-secondary/20 rounded-lg border p-6 transition-colors">
      <h2 className="mb-4 font-serif text-xl font-semibold">Order Summary</h2>
      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Subtotal</span>
          <span>{formatPrice(total)}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Shipping</span>
          <span className="text-green-600">Free</span>
        </div>
        <div className="border-border/40 mt-2 border-t pt-2">
          <div className="flex justify-between text-lg font-semibold">
            <span>Total</span>
            <span>{formatPrice(total)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
