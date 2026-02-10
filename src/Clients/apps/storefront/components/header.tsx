"use client";

import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";

import { useAtomValue } from "jotai";
import { Home, ShoppingBag } from "lucide-react";

import { Badge } from "@workspace/ui/components/badge";

import { basketItemsAtom } from "@/atoms/basket-atom";
import { HeaderAccountMenu } from "@/components/header-account-menu";
import { HeaderSearch, MobileSearch } from "@/components/header-search";
import { useSession } from "@/lib/auth-client";
import { APP_CONFIG } from "@/lib/constants";
import { isNavActive } from "@/lib/nav-utils";

const NAV_LINKS = [
  { href: "/shop", label: "Shop" },
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
  { href: "/about", label: "Our Story" },
] as const;

function formatItemCount(count: number): string {
  if (count === 0) return "";
  const suffix = count > 1 ? "s" : "";
  return ` (${count} item${suffix})`;
}

type BasketLinkProps = {
  readonly totalItems: number;
  readonly isBasketActive: boolean;
};

function BasketLink({ totalItems, isBasketActive }: BasketLinkProps) {
  const suffix = totalItems > 1 ? "s" : "";
  const itemSummary = totalItems > 0 ? `, ${totalItems} item${suffix}` : "";

  return (
    <Link
      href="/basket"
      className={`relative shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${isBasketActive ? "bg-secondary" : "hover:bg-secondary"}`}
      aria-label={`Shopping basket${itemSummary}`}
    >
      <ShoppingBag className="size-4 md:size-5" aria-hidden="true" />
      {totalItems > 0 && (
        <Badge
          className="absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full px-0 text-[10px] font-bold"
          aria-hidden="true"
        >
          {totalItems}
        </Badge>
      )}
      <span className="sr-only">Basket{formatItemCount(totalItems)}</span>
    </Link>
  );
}

export function Header() {
  const pathname = usePathname();
  const items = useAtomValue(basketItemsAtom);
  const totalItems = items.length;
  const { data: session } = useSession();

  return (
    <>
      <header className="bg-background/80 sticky top-0 z-50 hidden w-full border-b backdrop-blur-md md:block">
        <div className="container mx-auto flex h-20 items-center gap-4 px-4">
          <Link
            href="/"
            className="text-foreground flex shrink-0 items-center gap-1 text-lg font-bold tracking-tight md:gap-2 md:text-2xl"
          >
            <Image
              src="/logo.svg"
              alt={`${APP_CONFIG.name} Logo`}
              width={32}
              height={32}
              className="h-6 w-6 md:h-8 md:w-8"
            />
            <span className="hidden sm:inline">{APP_CONFIG.name}</span>
          </Link>

          <nav
            className="hidden flex-1 items-center justify-center gap-8 text-sm font-medium md:flex"
            aria-label="Main navigation"
          >
            {NAV_LINKS.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={`border-b-2 pb-1 transition-colors ${
                  isNavActive(pathname, link.href)
                    ? "text-primary border-primary"
                    : "text-foreground hover:text-primary border-b-2 border-transparent"
                }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          <div className="ml-auto flex items-center gap-2 md:gap-5">
            <HeaderSearch />

            <Link
              href="/"
              className={`shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${pathname === "/" ? "bg-secondary" : "hover:bg-secondary"}`}
              aria-label="Home"
            >
              <Home className="size-4 md:size-5" aria-hidden="true" />
              <span className="sr-only">Home</span>
            </Link>

            <HeaderAccountMenu
              isAccountActive={isNavActive(pathname, "/account")}
            />

            {session?.user && (
              <BasketLink
                totalItems={totalItems}
                isBasketActive={isNavActive(pathname, "/basket")}
              />
            )}
          </div>
        </div>
      </header>
      <header
        className="bg-background/80 sticky top-0 z-50 w-full border-b backdrop-blur-md md:hidden"
        role="banner"
      >
        <div className="container mx-auto flex h-16 items-center gap-2 px-3">
          <MobileSearch />
        </div>
      </header>
    </>
  );
}
