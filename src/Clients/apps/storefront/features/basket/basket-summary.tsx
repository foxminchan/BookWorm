import { ArrowRight, Loader2 } from "lucide-react";

import { Button } from "@workspace/ui/components/button";
import { Card } from "@workspace/ui/components/card";
import { Separator } from "@workspace/ui/components/separator";

type BasketSummaryProps = {
  subtotal: number;
  shipping: number;
  total: number;
  isCheckingOut: boolean;
  onCheckout: () => void;
};

export default function BasketSummary({
  subtotal,
  shipping,
  total,
  isCheckingOut,
  onCheckout,
}: BasketSummaryProps) {
  return (
    <div className="lg:col-span-4">
      <Card className="sticky top-32 border-none bg-white p-8 shadow-none dark:bg-gray-800">
        <h2 className="mb-6 font-serif text-2xl font-medium">Order Summary</h2>
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
            className="group mt-4 h-12 w-full rounded-full text-lg disabled:opacity-80"
          >
            {isCheckingOut ? (
              <>
                <Loader2 className="mr-2 size-5 animate-spin" />
                Processing...
              </>
            ) : (
              <>
                Checkout{" "}
                <ArrowRight className="ml-2 size-5 transition-transform group-hover:translate-x-1" />
              </>
            )}
          </Button>
          <p className="text-muted-foreground mt-4 text-center text-xs italic">
            Taxes calculated at checkout
          </p>
        </div>
      </Card>
    </div>
  );
}
