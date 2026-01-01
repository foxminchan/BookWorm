import { Button } from "@workspace/ui/components/button";
import { ArrowRight } from "lucide-react";
import Link from "next/link";

type ConfirmationActionsProps = {
  orderId: string;
};

export function ConfirmationActions({ orderId }: ConfirmationActionsProps) {
  return (
    <div className="flex flex-col sm:flex-row items-center justify-center gap-6">
      <Button
        asChild
        size="lg"
        className="rounded-full h-14 px-12 text-lg group"
      >
        <Link href="/shop">
          Continue Shopping{" "}
          <ArrowRight className="ml-3 size-5 group-hover:translate-x-1 transition-transform" />
        </Link>
      </Button>
      <Button
        asChild
        variant="outline"
        size="lg"
        className="rounded-full h-14 px-12 text-lg border-primary/20 hover:bg-primary/5 bg-transparent"
      >
        <Link href={`/account/orders/${orderId}`}>View Order</Link>
      </Button>
    </div>
  );
}
