import Link from "next/link";

import { ChevronRight } from "lucide-react";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";

import { getOrderStatusColorBordered } from "@/lib/pattern";

type Order = {
  id: string;
  status: OrderStatus;
  date: string;
  total: number;
};

type OrdersListProps = {
  orders: Order[];
};

export default function OrdersList({ orders }: OrdersListProps) {
  const formatter = new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  });
  return (
    <div className="border-border/40 bg-border/40 space-y-px border">
      {orders.map((order) => (
        <Link key={order.id} href={`/account/orders/${order.id}`}>
          <div className="group bg-background hover:bg-secondary/20 border-border/40 cursor-pointer border-b p-8 transition-all duration-300 last:border-b-0">
            <div className="flex items-start justify-between gap-6">
              <div className="min-w-0 flex-1">
                <div className="mb-6 flex items-center gap-4">
                  <div>
                    <p className="text-muted-foreground mb-1 text-xs tracking-widest uppercase">
                      Order ID
                    </p>
                    <h3 className="font-serif text-2xl font-medium">
                      {order.id}
                    </h3>
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
                    <p className="font-medium">
                      {new Date(order.date).toLocaleDateString("en-US", {
                        year: "numeric",
                        month: "long",
                        day: "numeric",
                      })}
                    </p>
                  </div>
                  <div>
                    <p className="text-muted-foreground mb-2 text-xs tracking-widest uppercase">
                      Total Amount
                    </p>
                    <p className="font-serif text-lg font-medium">
                      {formatter.format(order.total)}
                    </p>
                  </div>
                  <div className="col-span-2 flex items-end md:col-span-1">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="text-primary hover:text-primary/80 -ml-2 gap-2 font-medium transition-transform group-hover:translate-x-1"
                    >
                      View Details
                      <ChevronRight className="size-4" />
                    </Button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </Link>
      ))}
    </div>
  );
}
