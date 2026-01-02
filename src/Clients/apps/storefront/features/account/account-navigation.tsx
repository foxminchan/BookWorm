import Link from "next/link";

import { ChevronRight } from "lucide-react";

export default function AccountNavigation() {
  return (
    <div className="border-border/40 divide-border/40 divide-y overflow-hidden rounded-lg border">
      <Link href="/account/orders" className="block">
        <div className="hover:bg-secondary/20 flex items-center justify-between p-4 transition-colors">
          <span className="font-medium">Order History</span>
          <ChevronRight className="text-muted-foreground size-4" />
        </div>
      </Link>
    </div>
  );
}
