import { Button } from "@workspace/ui/components/button";
import { ArrowLeft } from "lucide-react";
import Link from "next/link";
import type { OrderStatus } from "@workspace/types/ordering/orders";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@workspace/ui/components/select";

const STATUS_OPTIONS: (OrderStatus | "All")[] = [
  "All",
  "New",
  "Completed",
  "Cancelled",
];

type OrdersHeaderProps = {
  selectedStatus: OrderStatus | "All";
  onStatusChange: (status: OrderStatus | "All") => void;
};

export function OrdersHeader({
  selectedStatus,
  onStatusChange,
}: OrdersHeaderProps) {
  return (
    <div className="mb-12 flex flex-col md:flex-row md:items-end justify-between gap-8">
      <div className="flex-1">
        <Link href="/account">
          <Button
            variant="ghost"
            className="mb-6 gap-2 -ml-2 text-muted-foreground hover:text-foreground"
          >
            <ArrowLeft className="size-4" />
            Back to Account
          </Button>
        </Link>
        <div className="space-y-2">
          <h1 className="font-serif text-5xl font-medium text-balance">
            Order History
          </h1>
          <p className="text-lg text-muted-foreground">
            Track and manage all your purchases
          </p>
        </div>
      </div>

      <div className="w-full md:w-56">
        <label className="text-xs font-semibold uppercase tracking-widest text-muted-foreground mb-3 block">
          Filter Orders
        </label>
        <Select value={selectedStatus} onValueChange={onStatusChange}>
          <SelectTrigger className="border-2 hover:border-primary/50 transition-colors">
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
