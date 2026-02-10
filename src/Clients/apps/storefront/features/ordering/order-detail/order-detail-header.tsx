import Link from "next/link";

import { ArrowLeft } from "lucide-react";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";

import { formatDate } from "@/lib/format";
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
}: Readonly<OrderDetailHeaderProps>) {
  return (
    <div className="mb-8">
      <Button variant="ghost" className="mb-4 -ml-2 gap-2" asChild>
        <Link href="/account/orders">
          <ArrowLeft className="size-4" />
          Back to Orders
        </Link>
      </Button>
      <div className="mb-2 flex flex-wrap items-center gap-3">
        <h1 className="font-serif text-4xl">Order {orderId}</h1>
        <Badge className={`${getOrderStatusColor(status)} border-0 text-sm`}>
          {status}
        </Badge>
      </div>
      <p className="text-muted-foreground">Placed on {formatDate(date)}</p>
    </div>
  );
}
