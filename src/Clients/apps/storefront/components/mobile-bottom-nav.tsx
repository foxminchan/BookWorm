"use client";

import { useEffect, useRef, useState } from "react";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { useAtomValue } from "jotai";
import { Home, Menu, ShoppingBag, ShoppingCart, User } from "lucide-react";

import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";

import { basketItemCountAtom } from "@/atoms/basket-atom";
import { useSession } from "@/lib/auth-client";

export function MobileBottomNav() {
  const pathname = usePathname();
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const itemCount = useAtomValue(basketItemCountAtom);
  const { data: session } = useSession();
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (!isMenuOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") {
        setIsMenuOpen(false);
      }
    };

    document.addEventListener("keydown", handleKeyDown);
    return () => document.removeEventListener("keydown", handleKeyDown);
  }, [isMenuOpen]);

  const isActive = (path: string) =>
    pathname === path || pathname.startsWith(path + "/");

  const navigationItems = [
    { href: "/", icon: Home, label: "Home" },
    { href: "/shop", icon: ShoppingBag, label: "Shop" },
    ...(session?.user
      ? [{ href: "/basket", icon: ShoppingCart, label: "Basket" }]
      : []),
    { href: "/account", icon: User, label: "Account" },
  ];

  const additionalMenuItems = [
    { href: "/categories", label: "Categories" },
    { href: "/publishers", label: "Publishers" },
    { href: "/about", label: "Our Story" },
  ];

  return (
    <>
      <nav
        className="bg-background fixed right-0 bottom-0 left-0 z-40 flex h-16 items-center justify-around border-t md:hidden"
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
                      {itemCount}
                    </Badge>
                    <span className="sr-only">{itemCount} items in basket</span>
                  </>
                )}
              </div>
              <span className="text-[10px]" aria-hidden="true">
                {item.label}
              </span>
            </Link>
          );
        })}
        <Button
          variant="ghost"
          onClick={() => setIsMenuOpen(!isMenuOpen)}
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
          {/* Backdrop */}
          <div
            className="fixed inset-0 z-30 bg-black/20 md:hidden"
            onClick={() => setIsMenuOpen(false)}
            aria-hidden="true"
          />
          <div
            ref={menuRef}
            className="bg-background fixed right-0 bottom-16 left-0 z-40 border-t md:hidden"
            role="navigation"
            aria-label="Additional navigation menu"
          >
            <div className="flex flex-col gap-2 p-3 text-sm" role="menu">
              {additionalMenuItems.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="hover:bg-secondary rounded px-3 py-2 transition-colors"
                  onClick={() => setIsMenuOpen(false)}
                  role="menuitem"
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </div>
        </>
      )}
    </>
  );
}
