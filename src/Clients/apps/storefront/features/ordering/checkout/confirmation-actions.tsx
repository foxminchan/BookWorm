import Link from "next/link";

import { ArrowRight } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

type ConfirmationActionsProps = {
  orderId: string;
};

export default function ConfirmationActions({
  orderId,
}: Readonly<ConfirmationActionsProps>) {
  return (
    <div className="flex flex-col items-center justify-center gap-6 sm:flex-row">
      <Button
        asChild
        size="lg"
        className="group h-14 rounded-full px-12 text-lg"
      >
        <Link href="/shop">
          Continue Shopping{" "}
          <ArrowRight className="ml-3 size-5 transition-transform group-hover:translate-x-1" />
        </Link>
      </Button>
      <Button
        asChild
        variant="outline"
        size="lg"
        className="border-primary/20 hover:bg-primary/5 h-14 rounded-full bg-transparent px-12 text-lg"
      >
        <Link href={`/account/orders/${orderId}`}>View Order</Link>
      </Button>
    </div>
  );
}
