import Link from "next/link";

import { ChevronRight } from "lucide-react";

import { STOREFRONT_ACCOUNT_SECTIONS } from "@/config/nav-config";

export default function AccountNavigation() {
  return (
    <div className="border-border/40 divide-border/40 divide-y overflow-hidden rounded-lg border">
      {STOREFRONT_ACCOUNT_SECTIONS.map((item) => (
        <Link key={item.href} href={item.href} className="block">
          <div className="hover:bg-secondary/20 flex items-center justify-between p-4 transition-colors">
            <span className="font-medium">{item.label}</span>
            <ChevronRight className="text-muted-foreground size-4" />
          </div>
        </Link>
      ))}
    </div>
  );
}
