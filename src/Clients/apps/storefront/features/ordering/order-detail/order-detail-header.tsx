import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import { getOrderStatusColor } from "@/lib/pattern";

type OrderDetailHeaderProps = {
  orderId: string;
  status: OrderStatus;
  date: string;
};

export default function OrderDetailHeader({
  orderId,
  status,
  date,
}: OrderDetailHeaderProps) {
  return (
    <div className="mb-8">
      <Link href="/account/orders">
        <Button variant="ghost" className="mb-4 gap-2 -ml-2">
          <ArrowLeft className="size-4" />
          Back to Orders
        </Button>
      </Link>
      <div className="flex flex-wrap items-center gap-3 mb-2">
        <h1 className="font-serif text-4xl">Order {orderId}</h1>
        <Badge className={`${getOrderStatusColor(status)} border-0 text-sm`}>
          {status}
        </Badge>
      </div>
      <p className="text-muted-foreground">
        Placed on{" "}
        {new Date(date).toLocaleDateString("en-US", {
          dateStyle: "long",
        })}
      </p>
    </div>
  );
}
