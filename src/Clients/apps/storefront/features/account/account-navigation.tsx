import Link from "next/link";
import { ChevronRight } from "lucide-react";

export default function AccountNavigation() {
  return (
    <div className="border border-border/40 rounded-lg overflow-hidden divide-y divide-border/40">
      <Link href="/account/orders" className="block">
        <div className="p-4 hover:bg-secondary/20 transition-colors flex items-center justify-between">
          <span className="font-medium">Order History</span>
          <ChevronRight className="size-4 text-muted-foreground" />
        </div>
      </Link>
    </div>
  );
}
