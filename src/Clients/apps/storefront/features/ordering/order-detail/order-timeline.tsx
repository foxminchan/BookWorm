"use client";

import { format } from "date-fns";
import { Clock } from "lucide-react";

import useOrderHistory from "@workspace/api-hooks/ordering/orders/useOrderHistory";
import type { OrderStatus } from "@workspace/types/ordering/orders";

type OrderTimelineProps = Readonly<{
  orderId: string;
}>;

const eventLabels: Record<OrderStatus, string> = {
  New: "Order Placed",
  Completed: "Order Completed",
  Cancelled: "Order Cancelled",
};

export default function OrderTimeline({ orderId }: OrderTimelineProps) {
  const { data: history, isError } = useOrderHistory(orderId, {
    retry: false,
  });

  if (isError || !history || history.length === 0) {
    return null;
  }

  return (
    <div className="border-border/40 bg-background hover:bg-secondary/20 rounded-lg border p-6 transition-colors">
      <h2 className="mb-4 flex items-center gap-2 font-serif text-xl font-semibold">
        <Clock className="size-5" />
        Order Timeline
      </h2>
      <ol className="space-y-3">
        {history.map((event) => (
          <li
            key={`${event.sequence}-${event.version}`}
            className="flex items-center justify-between gap-4 text-sm"
          >
            <div className="flex items-center gap-2">
              <span className="bg-primary size-2 rounded-full" />
              <span className="font-medium">
                {event.status ? eventLabels[event.status] : event.eventType}
              </span>
            </div>
            <span className="text-muted-foreground text-xs">
              {format(new Date(event.timestamp), "MMM dd, yyyy HH:mm")}
            </span>
          </li>
        ))}
      </ol>
    </div>
  );
}
