"use client";

import { useCallback, useRef, useState } from "react";

import Image from "next/image";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";

import { useAtomValue } from "jotai";
import { Home, Search, ShoppingBag, User } from "lucide-react";
import { useForm } from "react-hook-form";

import { Badge } from "@workspace/ui/components/badge";
import { Button } from "@workspace/ui/components/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
} from "@workspace/ui/components/form";
import { Input } from "@workspace/ui/components/input";
import { Label } from "@workspace/ui/components/label";

import { basketItemsAtom } from "@/atoms/basket-atom";
import { signOut, useSession } from "@/lib/auth-client";

const NAV_LINKS = [
  { href: "/shop", label: "Shop" },
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
  { href: "/about", label: "Our Story" },
] as const;

const AUTHENTICATED_MENU_ITEMS = [
  { href: "/account", label: "My Profile" },
  { href: "/account/orders", label: "Order History" },
] as const;

const GUEST_MENU_ITEMS = [
  { href: "/login", label: "Login" },
  { href: "/register", label: "Register" },
] as const;

export function Header() {
  const router = useRouter();
  const pathname = usePathname();
  const items = useAtomValue(basketItemsAtom);
  const totalItems = items.length;
  const { data: session } = useSession();
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const searchForm = useForm<{ search: string }>({
    defaultValues: { search: "" },
  });
  const [isAccountOpen, setIsAccountOpen] = useState(false);
  const hideAccountTimeoutRef = useRef<NodeJS.Timeout | null>(null);
  const searchHoverTimeoutRef = useRef<NodeJS.Timeout | null>(null);

  const handleSearch = (data: { search: string }) => {
    if (data.search.trim()) {
      router.push(`/shop?search=${encodeURIComponent(data.search)}`);
      setIsSearchOpen(false);
      searchForm.reset();
    }
  };

  const handleLogout = async () => {
    setIsAccountOpen(false);
    await signOut();
    router.push("/");
  };

  const handleAccountMouseEnter = useCallback(() => {
    if (hideAccountTimeoutRef.current) {
      clearTimeout(hideAccountTimeoutRef.current);
      hideAccountTimeoutRef.current = null;
    }
    setIsAccountOpen(true);
  }, []);

  const handleAccountMouseLeave = useCallback(() => {
    hideAccountTimeoutRef.current = setTimeout(() => {
      setIsAccountOpen(false);
    }, 200);
  }, []);

  const handleSearchMouseEnter = useCallback(() => {
    if (searchHoverTimeoutRef.current) {
      clearTimeout(searchHoverTimeoutRef.current);
      searchHoverTimeoutRef.current = null;
    }
    setIsSearchOpen(true);
  }, []);

  const handleSearchMouseLeave = useCallback(() => {
    searchHoverTimeoutRef.current = setTimeout(() => {
      setIsSearchOpen(false);
    }, 150);
  }, []);

  // Helper function to check if link is active
  const isActive = (href: string) => {
    if (href === "/shop") return pathname.startsWith("/shop");
    if (href === "/categories") return pathname.startsWith("/categories");
    if (href === "/publishers") return pathname.startsWith("/publishers");
    if (href === "/about") return pathname === "/about";
    return pathname === href;
  };

  return (
    <>
      <header className="bg-background/80 sticky top-0 z-50 hidden w-full border-b backdrop-blur-md md:block">
        <div className="container mx-auto flex h-20 items-center gap-4 px-4">
          {/* Left: Logo */}
          <Link
            href="/"
            className="text-foreground flex shrink-0 items-center gap-1 text-lg font-bold tracking-tight md:gap-2 md:text-2xl"
          >
            <Image
              src="/logo.svg"
              alt="BookWorm Logo"
              width={32}
              height={32}
              className="h-6 w-6 md:h-8 md:w-8"
            />
            <span className="hidden sm:inline">BookWorm</span>
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
                  isActive(link.href)
                    ? "text-primary border-primary"
                    : "text-foreground hover:text-primary border-b-2 border-transparent"
                }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          <div className="ml-auto flex items-center gap-2 md:gap-5">
            <div
              className="relative"
              onMouseEnter={handleSearchMouseEnter}
              onMouseLeave={handleSearchMouseLeave}
            >
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="h-9 w-9 rounded-full"
                onClick={() => setIsSearchOpen(!isSearchOpen)}
                onKeyDown={(e) => {
                  if (e.key === "Escape" && isSearchOpen) {
                    setIsSearchOpen(false);
                  }
                }}
                aria-label="Search books"
                aria-expanded={isSearchOpen}
                aria-haspopup="true"
              >
                <Search
                  className="text-foreground/60 size-4 md:size-5"
                  aria-hidden="true"
                />
              </Button>
              {isSearchOpen && (
                <div className="bg-background border-foreground/10 animate-in fade-in slide-in-from-top-2 absolute top-full right-0 mt-2 w-48 rounded-lg border p-2 shadow-lg duration-200 md:w-64">
                  <Form {...searchForm}>
                    <form onSubmit={searchForm.handleSubmit(handleSearch)}>
                      <FormField
                        control={searchForm.control}
                        name="search"
                        render={({ field }) => (
                          <FormItem>
                            <FormControl>
                              <Label
                                htmlFor="header-search-input"
                                className="sr-only"
                              >
                                Search books
                              </Label>
                              <Input
                                id="header-search-input"
                                placeholder="Search books..."
                                autoFocus
                                aria-label="Search books"
                                className="border-foreground/20 focus:border-primary placeholder:text-foreground/40 border-x-0 border-t-0 border-b bg-transparent px-3 py-2 text-sm"
                                {...field}
                              />
                            </FormControl>
                          </FormItem>
                        )}
                      />
                    </form>
                  </Form>
                </div>
              )}
            </div>

            <Link
              href="/"
              className={`shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${isActive("/") ? "bg-secondary" : "hover:bg-secondary"}`}
              aria-label="Home"
            >
              <Home className="size-4 md:size-5" aria-hidden="true" />
              <span className="sr-only">Home</span>
            </Link>

            <div
              className="relative"
              onMouseEnter={handleAccountMouseEnter}
              onMouseLeave={handleAccountMouseLeave}
            >
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className={`h-9 w-9 rounded-full ${
                  isActive("/account") ? "bg-secondary" : ""
                }`}
                onKeyDown={(e) => {
                  if (e.key === "Escape" && isAccountOpen) {
                    setIsAccountOpen(false);
                  }
                }}
                aria-label="Account menu"
                aria-expanded={isAccountOpen}
                aria-haspopup="true"
              >
                <User className="size-4 md:size-5" aria-hidden="true" />
                <span className="sr-only">Account</span>
              </Button>

              {/* Dropdown Menu */}
              {isAccountOpen && (
                <div
                  className="bg-background border-foreground/10 animate-in fade-in slide-in-from-top-2 absolute right-0 mt-2 w-40 rounded-lg border py-2 text-sm shadow-lg duration-200 md:w-48"
                  role="menu"
                  aria-label="Account menu"
                >
                  {session?.user ? (
                    <>
                      {AUTHENTICATED_MENU_ITEMS.map((item) => (
                        <Link
                          key={item.href}
                          href={item.href}
                          className="hover:bg-secondary block px-4 py-2 transition-colors"
                          role="menuitem"
                        >
                          {item.label}
                        </Link>
                      ))}
                      <Button
                        variant="ghost"
                        onClick={handleLogout}
                        className="text-primary h-auto w-full justify-start px-4 py-2 font-normal"
                        role="menuitem"
                      >
                        Logout
                      </Button>
                    </>
                  ) : (
                    <>
                      {GUEST_MENU_ITEMS.map((item) => (
                        <Link
                          key={item.href}
                          href={item.href}
                          className="hover:bg-secondary block px-4 py-2 transition-colors"
                          role="menuitem"
                        >
                          {item.label}
                        </Link>
                      ))}
                    </>
                  )}
                </div>
              )}
            </div>

            {session?.user && (
              <Link
                href="/basket"
                className={`relative shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${isActive("/basket") ? "bg-secondary" : "hover:bg-secondary"}`}
                aria-label={`Shopping basket${totalItems > 0 ? `, ${totalItems} item${totalItems > 1 ? "s" : ""}` : ""}`}
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
                <span className="sr-only">
                  Basket
                  {totalItems > 0
                    ? ` (${totalItems} item${totalItems > 1 ? "s" : ""})`
                    : ""}
                </span>
              </Link>
            )}
          </div>
        </div>
      </header>
      <header
        className="bg-background/80 sticky top-0 z-50 w-full border-b backdrop-blur-md md:hidden"
        role="banner"
      >
        <div className="container mx-auto flex h-16 items-center gap-2 px-3">
          {/* Search Form */}
          <Form {...searchForm}>
            <form
              onSubmit={searchForm.handleSubmit(handleSearch)}
              className="hover:bg-secondary relative flex w-full items-center rounded-full p-2 transition-colors"
              role="search"
              aria-label="Search books"
            >
              <Label htmlFor="mobile-search-input" className="sr-only">
                Search books
              </Label>
              <Search
                className="text-foreground/60 pointer-events-none size-4 shrink-0"
                aria-hidden="true"
              />
              <FormField
                control={searchForm.control}
                name="search"
                render={({ field }) => (
                  <FormItem className="flex-1">
                    <FormControl>
                      <Input
                        id="mobile-search-input"
                        placeholder="Search books..."
                        className="w-full border-0 bg-transparent py-1 pl-2 text-sm outline-none focus-visible:ring-0"
                        {...field}
                      />
                    </FormControl>
                  </FormItem>
                )}
              />
            </form>
          </Form>
        </div>
      </header>
    </>
  );
}
