import { Button } from "@workspace/ui/components/button";
import { Badge } from "@workspace/ui/components/badge";
import { ChevronRight } from "lucide-react";
import Link from "next/link";
import type { OrderStatus } from "@workspace/types/ordering/orders";
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
  return (
    <div className="space-y-px border border-border/40 bg-border/40">
      {orders.map((order) => (
        <Link key={order.id} href={`/account/orders/${order.id}`}>
          <div className="group bg-background hover:bg-secondary/20 transition-all duration-300 p-8 cursor-pointer border-b border-border/40 last:border-b-0">
            <div className="flex items-start justify-between gap-6">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-4 mb-6">
                  <div>
                    <p className="text-xs uppercase tracking-widest text-muted-foreground mb-1">
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

                <div className="grid grid-cols-2 md:grid-cols-3 gap-6 pt-6 border-t border-border">
                  <div>
                    <p className="text-xs uppercase tracking-widest text-muted-foreground mb-2">
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
                    <p className="text-xs uppercase tracking-widest text-muted-foreground mb-2">
                      Total Amount
                    </p>
                    <p className="font-serif text-lg font-medium">
                      ${order.total.toFixed(2)}
                    </p>
                  </div>
                  <div className="col-span-2 md:col-span-1 flex items-end">
                    <Button
                      variant="ghost"
                      size="sm"
                      className="gap-2 text-primary hover:text-primary/80 font-medium -ml-2 group-hover:translate-x-1 transition-transform"
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
