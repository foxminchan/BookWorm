"use client";

import { useCallback, useEffect, useMemo, useState } from "react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { useAtomValue } from "jotai";
import type { LucideIcon } from "lucide-react";
import { Home, Menu, ShoppingBag, ShoppingCart, User } from "lucide-react";

import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";

import { basketItemCountAtom } from "@/atoms/basket-atom";
import { useSession } from "@/lib/auth-client";
import { isNavActive } from "@/lib/nav-utils";

type NavItem = {
  readonly href: string;
  readonly icon: LucideIcon;
  readonly label: string;
};

const BASE_NAV_ITEMS: readonly NavItem[] = [
  { href: "/", icon: Home, label: "Home" },
  { href: "/shop", icon: ShoppingBag, label: "Shop" },
] as const;

const BASKET_NAV_ITEM: NavItem = {
  href: "/basket",
  icon: ShoppingCart,
  label: "Basket",
} as const;

const ACCOUNT_NAV_ITEM: NavItem = {
  href: "/account",
  icon: User,
  label: "Account",
} as const;

const MORE_MENU_ITEMS = [
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
  { href: "/about", label: "Our Story" },
] as const;

function useEscapeKey(isActive: boolean, onEscape: () => void) {
  useEffect(() => {
    if (!isActive) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        onEscape();
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isActive, onEscape]);
}

type NavLinkItemProps = {
  readonly item: NavItem;
  readonly active: boolean;
  readonly badgeCount?: number;
  readonly onClick?: () => void;
};

function NavLinkItem({ item, active, badgeCount, onClick }: NavLinkItemProps) {
  const Icon = item.icon;
  const showBadge = badgeCount !== undefined && badgeCount > 0;

  return (
    <Link
      href={item.href}
      onClick={onClick}
      className={`relative flex h-full flex-1 flex-col items-center justify-center gap-1 text-xs transition-colors ${
        active
          ? "text-primary border-primary border-t-2"
          : "text-muted-foreground hover:text-foreground"
      }`}
      aria-current={active ? "page" : undefined}
      title={item.label}
    >
      <div className="relative">
        <Icon className="size-5" aria-hidden="true" />
        {showBadge && (
          <>
            <Badge
              className="absolute -top-2 -right-2 flex h-5 w-5 items-center justify-center p-0 text-[10px]"
              aria-hidden="true"
            >
              {badgeCount}
            </Badge>
            <span className="sr-only">{badgeCount} items in basket</span>
          </>
        )}
      </div>
      <span className="text-[10px]" aria-hidden="true">
        {item.label}
      </span>
    </Link>
  );
}

export function MobileBottomNav() {
  const pathname = usePathname();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const itemCount = useAtomValue(basketItemCountAtom);
  const { data: session } = useSession();

  const closeMenu = useCallback(() => setIsMenuOpen(false), []);
  useEscapeKey(isMenuOpen, closeMenu);

  const navigationItems = useMemo<readonly NavItem[]>(() => {
    const items = [...BASE_NAV_ITEMS];
    if (session?.user) {
      items.push(BASKET_NAV_ITEM);
    }
    items.push(ACCOUNT_NAV_ITEM);
    return items;
  }, [session?.user]);

  return (
    <>
      <nav
        className="bg-background fixed right-0 bottom-0 left-0 z-40 flex h-16 items-center justify-around border-t md:hidden"
        aria-label="Mobile main navigation"
      >
        {navigationItems.map((item) => (
          <NavLinkItem
            key={item.href}
            item={item}
            active={isNavActive(pathname, item.href)}
            badgeCount={item.href === "/basket" ? itemCount : undefined}
            onClick={closeMenu}
          />
        ))}
        <Button
          variant="ghost"
          onClick={() => setIsMenuOpen((prev) => !prev)}
          className="text-muted-foreground hover:text-foreground flex h-full flex-1 flex-col items-center justify-center gap-1 p-0 text-xs"
          aria-label={
            isMenuOpen ? "Close additional menu" : "Open additional menu"
          }
          aria-expanded={isMenuOpen}
          title="More options"
        >
          <Menu className="size-5" aria-hidden="true" />
          <span className="text-[10px]">More</span>
        </Button>
      </nav>

      {isMenuOpen && (
        <>
          <div
            className="fixed inset-0 z-30 bg-black/20 md:hidden"
            onClick={closeMenu}
            aria-hidden="true"
          />
          <nav
            className="bg-background fixed right-0 bottom-16 left-0 z-40 border-t md:hidden"
            aria-label="Additional navigation menu"
          >
            <div className="flex flex-col gap-2 p-3 text-sm" role="menu">
              {MORE_MENU_ITEMS.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="hover:bg-secondary rounded px-3 py-2 transition-colors"
                  onClick={closeMenu}
                  role="menuitem"
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </nav>
        </>
      )}
    </>
  );
}
