type OrderSummaryProps = {
  total: number;
};

export default function OrderSummary({ total }: OrderSummaryProps) {
  return (
    <div className="border border-border/40 rounded-lg p-6 bg-background hover:bg-secondary/20 transition-colors">
      <h2 className="font-serif text-xl font-semibold mb-4">Order Summary</h2>
      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Subtotal</span>
          <span>${total.toFixed(2)}</span>
        </div>
        <div className="flex justify-between text-sm">
          <span className="text-muted-foreground">Shipping</span>
          <span className="text-green-600">Free</span>
        </div>
        <div className="border-t border-border/40 pt-2 mt-2">
          <div className="flex justify-between font-semibold text-lg">
            <span>Total</span>
            <span>${total.toFixed(2)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
