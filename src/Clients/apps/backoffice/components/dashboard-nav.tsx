"use client";

import type React from "react";
import { useCallback } from "react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import {
  BookOpen,
  LayoutDashboard,
  Package,
  PenTool,
  ShoppingCart,
  Star,
  Tag,
  UsersIcon,
} from "lucide-react";

import { Button } from "@workspace/ui/components/button";

import { useLogout } from "@/hooks/useLogout";

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
      { href: "/orders", icon: ShoppingCart, label: "Orders" },
    ],
  },
  {
    title: "Ratings & Reviews",
    items: [{ href: "/reviews", icon: Star, label: "Reviews" }],
  },
] as const;

export function DashboardNav() {
  const pathname = usePathname();
  const { logout } = useLogout();

  const isActive = useCallback(
    (href: string) => {
      if (href === "/") {
        return pathname === "/";
      }
      return pathname === href || pathname.startsWith(`${href}/`);
    },
    [pathname],
  );

  return (
    <nav
      className="border-border bg-background flex w-64 flex-col border-r px-4 py-6"
      aria-label="Main navigation"
    >
      <div className="mb-8">
        <h1 className="text-foreground text-lg font-bold">BookWorm</h1>
      </div>

      <ul className="flex-1 space-y-2">
        {navigations.map((section) => (
          <li
            key={section.title ?? "root"}
            className={section.title ? "mt-6" : ""}
          >
            {section.title && (
              <p className="text-muted-foreground px-3 py-2 text-xs font-semibold tracking-wider uppercase">
                {section.title}
              </p>
            )}
            <ul className="space-y-1">
              {section.items.map((item) => (
                <li key={item.href}>
                  <NavLink
                    href={item.href}
                    icon={<item.icon className="h-5 w-5" aria-hidden="true" />}
                    label={item.label}
                    active={isActive(item.href)}
                  />
                </li>
              ))}
            </ul>
          </li>
        ))}
      </ul>

      <div className="border-border border-t pt-4">
        <Button
          variant="outline"
          className="w-full bg-transparent text-xs"
          onClick={logout}
          aria-label="Logout from admin portal"
        >
          Logout
        </Button>
      </div>
    </nav>
  );
}

type NavLinkProps = Readonly<{
  href: string;
  icon: React.ReactNode;
  label: string;
  active: boolean;
}>;

function NavLink({ href, icon, label, active }: NavLinkProps) {
  return (
    <Button
      variant={active ? "default" : "ghost"}
      className="w-full justify-start gap-3"
      asChild
    >
      <Link href={href} aria-current={active ? "page" : undefined}>
        {icon}
        <span>{label}</span>
      </Link>
    </Button>
  );
}
