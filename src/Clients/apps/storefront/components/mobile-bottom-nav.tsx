"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { Home, ShoppingBag, ShoppingCart, User, Menu } from "lucide-react";
import { useState } from "react";
import { Badge } from "@workspace/ui/components/badge";
import { useAtomValue } from "jotai";
import { basketItemCountAtom } from "@/atoms/basket-atom";

export function MobileBottomNav() {
  const pathname = usePathname();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const itemCount = useAtomValue(basketItemCountAtom);

  const isActive = (path: string) =>
    pathname === path || pathname.startsWith(path + "/");

  const navigationItems = [
    { href: "/", icon: Home, label: "Home" },
    { href: "/shop", icon: ShoppingBag, label: "Shop" },
    { href: "/basket", icon: ShoppingCart, label: "Basket" },
    { href: "/account", icon: User, label: "Account" },
  ];

  return (
    <>
      <nav
        className="md:hidden fixed bottom-0 left-0 right-0 z-40 border-t bg-background flex items-center justify-around h-16"
        aria-label="Mobile main navigation"
      >
        {navigationItems.map((item) => {
          const Icon = item.icon;
          const active = isActive(item.href);
          const showBadge = item.href === "/basket" && itemCount > 0;
          return (
            <Link
              key={item.href}
              href={item.href}
              onClick={() => setIsMenuOpen(false)}
              className={`flex flex-col items-center justify-center flex-1 h-full gap-1 text-xs transition-colors relative ${
                active
                  ? "text-primary border-t-2 border-primary"
                  : "text-muted-foreground hover:text-foreground"
              }`}
              aria-current={active ? "page" : undefined}
              title={item.label}
            >
              <div className="relative">
                <Icon className="size-5" aria-hidden="true" />
                {showBadge && (
                  <Badge
                    className="absolute -top-2 -right-2 h-5 w-5 flex items-center justify-center p-0 text-[10px]"
                    aria-label={`${itemCount} items in basket`}
                  >
                    {itemCount}
                  </Badge>
                )}
              </div>
              <span className="text-[10px]">{item.label}</span>
            </Link>
          );
        })}
        <button
          onClick={() => setIsMenuOpen(!isMenuOpen)}
          className="flex flex-col items-center justify-center flex-1 h-full gap-1 text-xs text-muted-foreground hover:text-foreground transition-colors"
          aria-label={
            isMenuOpen ? "Close additional menu" : "Open additional menu"
          }
          aria-expanded={isMenuOpen}
          title="More options"
        >
          <Menu className="size-5" aria-hidden="true" />
          <span className="text-[10px]">More</span>
        </button>
      </nav>

      {isMenuOpen && (
        <div
          className="md:hidden fixed bottom-16 left-0 right-0 z-40 border-t bg-background"
          role="navigation"
          aria-label="Additional navigation"
        >
          <div className="flex flex-col p-3 gap-2 text-sm">
            <Link
              href="/categories"
              className="px-3 py-2 hover:bg-secondary rounded transition-colors"
              onClick={() => setIsMenuOpen(false)}
            >
              Categories
            </Link>
            <Link
              href="/publishers"
              className="px-3 py-2 hover:bg-secondary rounded transition-colors"
              onClick={() => setIsMenuOpen(false)}
            >
              Publishers
            </Link>
            <Link
              href="/about"
              className="px-3 py-2 hover:bg-secondary rounded transition-colors"
              onClick={() => setIsMenuOpen(false)}
            >
              Our Story
            </Link>
          </div>
        </div>
      )}
    </>
  );
}
