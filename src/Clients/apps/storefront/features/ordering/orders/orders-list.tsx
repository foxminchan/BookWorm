import Link from "next/link";

import { ChevronRight } from "lucide-react";

import type { Order } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { formatDate, formatPrice } from "@workspace/utils/format";

import { getOrderStatusColorBordered } from "@/lib/pattern";

type OrdersListProps = {
  orders: Order[];
};

function OrderCard({ order }: Readonly<{ order: Order }>) {
  return (
    <Link
      href={`/account/orders/${order.id}`}
      aria-label={`View order ${order.id}`}
    >
      <div className="group bg-background hover:bg-secondary/20 border-border/40 cursor-pointer border-b p-8 transition-all duration-300 last:border-b-0">
        <div className="flex items-start justify-between gap-6">
          <div className="min-w-0 flex-1">
            <div className="mb-6 flex items-center gap-4">
              <div>
                <p className="text-muted-foreground mb-1 text-xs tracking-widest uppercase">
                  Order ID
                </p>
                <h3 className="font-serif text-2xl font-medium">{order.id}</h3>
              </div>
              <Badge
                className={`${getOrderStatusColorBordered(order.status)} px-3 py-1 text-xs font-semibold`}
              >
                {order.status}
              </Badge>
            </div>

            <div className="border-border grid grid-cols-2 gap-6 border-t pt-6 md:grid-cols-3">
              <div>
                <p className="text-muted-foreground mb-2 text-xs tracking-widest uppercase">
                  Order Date
                </p>
                <p className="font-medium">{formatDate(order.date)}</p>
              </div>
              <div>
                <p className="text-muted-foreground mb-2 text-xs tracking-widest uppercase">
                  Total Amount
                </p>
                <p className="font-serif text-lg font-medium">
                  {formatPrice(order.total)}
                </p>
              </div>
              <div className="col-span-2 flex items-end md:col-span-1">
                <span
                  className="text-primary group-hover:text-primary/80 -ml-2 inline-flex items-center gap-2 text-sm font-medium transition-transform group-hover:translate-x-1"
                  aria-hidden="true"
                >
                  View Details
                  <ChevronRight className="size-4" />
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Link>
  );
}

export default function OrdersList({ orders }: Readonly<OrdersListProps>) {
  return (
    <div className="border-border/40 bg-border/40 space-y-px border">
      {orders.map((order) => (
        <OrderCard key={order.id} order={order} />
      ))}
    </div>
  );
}
