"use client";

import type React from "react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import {
  BookOpen,
  LayoutDashboard,
  Package,
  PenTool,
  Tag,
  UsersIcon,
} from "lucide-react";

import { Button } from "@workspace/ui/components/button";

const navigations = [
  {
    title: null,
    items: [{ href: "/", icon: LayoutDashboard, label: "Overview" }],
  },
  {
    title: "Catalog",
    items: [
      { href: "/books", icon: BookOpen, label: "Books" },
      { href: "/categories", icon: Tag, label: "Categories" },
      { href: "/authors", icon: PenTool, label: "Authors" },
      { href: "/publishers", icon: Package, label: "Publishers" },
    ],
  },
  {
    title: "Ordering",
    items: [
      { href: "/customers", icon: UsersIcon, label: "Customers" },
      { href: "/orders", icon: BookOpen, label: "Orders" },
    ],
  },
  {
    title: "Ratings & Reviews",
    items: [{ href: "/reviews", icon: UsersIcon, label: "Reviews" }],
  },
] as const;

export function DashboardNav() {
  const pathname = usePathname();

  const isActive = (href: string) => {
    if (href === "/") {
      return pathname === "/" || pathname === "/(admin)";
    }
    return pathname.includes(href);
  };

  return (
    <nav className="border-border bg-background flex w-64 flex-col border-r px-4 py-6">
      <div className="mb-8">
        <h1 className="text-foreground text-lg font-bold">BookWorm</h1>
      </div>

      <div className="flex-1 space-y-2">
        {navigations.map((section, sectionIndex) => (
          <div key={sectionIndex} className={section.title ? "mt-6" : ""}>
            {section.title && (
              <p className="text-muted-foreground px-3 py-2 text-xs font-semibold tracking-wider uppercase">
                {section.title}
              </p>
            )}
            {section.items.map((item) => (
              <NavLink
                key={item.href}
                href={item.href}
                icon={<item.icon className="h-5 w-5" />}
                label={item.label}
                active={isActive(item.href)}
              />
            ))}
          </div>
        ))}
      </div>

      <div className="border-border border-t pt-4">
        <Button variant="outline" className="w-full bg-transparent text-xs">
          Logout
        </Button>
      </div>
    </nav>
  );
}

function NavLink({
  href,
  icon,
  label,
  active,
}: {
  href: string;
  icon: React.ReactNode;
  label: string;
  active: boolean;
}) {
  return (
    <Link href={href}>
      <Button
        variant={active ? "default" : "ghost"}
        className="w-full justify-start gap-3"
        asChild
      >
        <div>
          {icon}
          {label}
        </div>
      </Button>
    </Link>
  );
}
