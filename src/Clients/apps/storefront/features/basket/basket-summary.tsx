import { Card } from "@workspace/ui/components/card";
import { Button } from "@workspace/ui/components/button";
import { Separator } from "@workspace/ui/components/separator";
import { ArrowRight, Loader2 } from "lucide-react";

type BasketSummaryProps = {
  subtotal: number;
  shipping: number;
  total: number;
  isCheckingOut: boolean;
  onCheckout: () => void;
};

export function BasketSummary({
  subtotal,
  shipping,
  total,
  isCheckingOut,
  onCheckout,
}: BasketSummaryProps) {
  return (
    <div className="lg:col-span-4">
      <Card className="border-none shadow-none bg-white dark:bg-gray-800 p-8 sticky top-32">
        <h2 className="text-2xl font-serif font-medium mb-6">Order Summary</h2>
        <div className="space-y-4">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Subtotal</span>
            <span className="font-medium">${subtotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Shipping</span>
            <span className="font-medium">${shipping.toFixed(2)}</span>
          </div>
          <Separator />
          <div className="flex justify-between text-lg font-bold">
            <span>Total</span>
            <span className="text-primary">${total.toFixed(2)}</span>
          </div>
          <Button
            onClick={onCheckout}
            disabled={isCheckingOut}
            className="w-full h-12 rounded-full text-lg mt-4 group disabled:opacity-80"
          >
            {isCheckingOut ? (
              <>
                <Loader2 className="mr-2 size-5 animate-spin" />
                Processing...
              </>
            ) : (
              <>
                Checkout{" "}
                <ArrowRight className="ml-2 size-5 group-hover:translate-x-1 transition-transform" />
              </>
            )}
          </Button>
          <p className="text-center text-xs text-muted-foreground mt-4 italic">
            Taxes calculated at checkout
          </p>
        </div>
      </Card>
    </div>
  );
}
