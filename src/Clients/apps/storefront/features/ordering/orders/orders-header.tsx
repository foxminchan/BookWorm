import Link from "next/link";

import { ArrowLeft } from "lucide-react";

import type { OrderStatus } from "@workspace/types/ordering/orders";
import { Button } from "@workspace/ui/components/button";
import { Label } from "@workspace/ui/components/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";

const STATUS_OPTIONS: readonly (OrderStatus | "All")[] = [
  "All",
  "New",
  "Completed",
  "Cancelled",
] as const;

type OrdersHeaderProps = {
  selectedStatus: OrderStatus | "All";
  onStatusChange: (status: OrderStatus | "All") => void;
};

export default function OrdersHeader({
  selectedStatus,
  onStatusChange,
}: Readonly<OrdersHeaderProps>) {
  return (
    <div className="mb-12 flex flex-col justify-between gap-8 md:flex-row md:items-end">
      <div className="flex-1">
        <Button
          asChild
          variant="ghost"
          className="text-muted-foreground hover:text-foreground mb-6 -ml-2 gap-2"
        >
          <Link href="/account">
            <ArrowLeft className="size-4" aria-hidden="true" />
            Back to Account
          </Link>
        </Button>
        <div className="space-y-2">
          <h1 className="font-serif text-5xl font-medium text-balance">
            Order History
          </h1>
          <p className="text-muted-foreground text-lg">
            Track and manage all your purchases
          </p>
        </div>
      </div>

      <div className="w-full md:w-56">
        <Label
          htmlFor="order-status-filter"
          className="text-muted-foreground mb-3 block text-xs font-semibold tracking-widest uppercase"
        >
          Filter Orders
        </Label>
        <Select value={selectedStatus} onValueChange={onStatusChange}>
          <SelectTrigger
            id="order-status-filter"
            className="hover:border-primary/50 border-2 transition-colors"
          >
            <SelectValue placeholder="Select status" />
          </SelectTrigger>
          <SelectContent>
            {STATUS_OPTIONS.map((status) => (
              <SelectItem key={status} value={status}>
                {status}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    </div>
  );
}
