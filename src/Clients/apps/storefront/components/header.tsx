"use client";

import type React from "react";
import { useState } from "react";

import Image from "next/image";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";

import { useAtomValue } from "jotai";
import { Home, Search, ShoppingBag, User } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

import { basketItemsAtom } from "@/atoms/basket-atom";
import { signOut, useSession } from "@/lib/auth-client";

export function Header() {
  const router = useRouter();
  const pathname = usePathname();
  const items = useAtomValue(basketItemsAtom);
  const totalItems = items.length;
  const { data: session } = useSession();
  const [isSearchOpen, setIsSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [isAccountOpen, setIsAccountOpen] = useState(false);
  const [hideAccountTimeout, setHideAccountTimeout] =
    useState<NodeJS.Timeout | null>(null);
  const [searchHoverTimeout, setSearchHoverTimeout] =
    useState<NodeJS.Timeout | null>(null);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      router.push(`/shop?search=${encodeURIComponent(searchQuery)}`);
      setIsSearchOpen(false);
      setSearchQuery("");
    }
  };

  const handleLogout = async () => {
    setIsAccountOpen(false);
    await signOut();
    router.push("/");
  };

  const handleAccountMouseEnter = () => {
    if (hideAccountTimeout) {
      clearTimeout(hideAccountTimeout);
      setHideAccountTimeout(null);
    }
    setIsAccountOpen(true);
  };

  const handleAccountMouseLeave = () => {
    const timeout = setTimeout(() => {
      setIsAccountOpen(false);
    }, 200);
    setHideAccountTimeout(timeout);
  };

  const handleSearchMouseEnter = () => {
    if (searchHoverTimeout) {
      clearTimeout(searchHoverTimeout);
      setSearchHoverTimeout(null);
    }
    setIsSearchOpen(true);
  };

  const handleSearchMouseLeave = () => {
    const timeout = setTimeout(() => {
      setIsSearchOpen(false);
    }, 150);
    setSearchHoverTimeout(timeout);
  };

  const navLinks = [
    { href: "/shop", label: "Shop" },
    { href: "/categories", label: "Categories" },
    { href: "/publishers", label: "Publishers" },
    { href: "/about", label: "Our Story" },
  ];

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
      <header
        className="bg-background/80 sticky top-0 z-50 hidden w-full border-b backdrop-blur-md md:block"
        role="banner"
      >
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
            aria-label="Main Navigation"
          >
            {navLinks.map((link) => (
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
              <form onSubmit={handleSearch}>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  className="h-9 w-9 rounded-full"
                  onClick={() => setIsSearchOpen(!isSearchOpen)}
                  aria-label="Toggle search"
                >
                  <Search className="text-foreground/60 size-4 md:size-5" />
                </Button>
                {isSearchOpen && (
                  <div className="bg-background border-foreground/10 animate-in fade-in slide-in-from-top-2 absolute top-full right-0 mt-2 w-48 rounded-lg border p-2 shadow-lg duration-200 md:w-64">
                    <input
                      type="text"
                      placeholder="Search books..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      autoFocus
                      className="border-foreground/20 focus:border-primary placeholder:text-foreground/40 w-full border-b bg-transparent px-3 py-2 text-sm outline-none"
                      aria-label="Search books"
                    />
                  </div>
                )}
              </form>
            </div>

            <Link
              href="/"
              className={`shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${isActive("/") ? "bg-secondary" : "hover:bg-secondary"}`}
              aria-label="Home"
            >
              <Home className="size-4 md:size-5" />
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
                aria-label="Account"
              >
                <User className="size-4 md:size-5" />
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
                      <Link
                        href="/account"
                        className="hover:bg-secondary block px-4 py-2 transition-colors"
                        role="menuitem"
                      >
                        My Profile
                      </Link>
                      <Link
                        href="/account/orders"
                        className="hover:bg-secondary block px-4 py-2 transition-colors"
                        role="menuitem"
                      >
                        Order History
                      </Link>
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
                      <Link
                        href="/login"
                        className="hover:bg-secondary block px-4 py-2 transition-colors"
                        role="menuitem"
                      >
                        Login
                      </Link>
                      <Link
                        href="/register"
                        className="hover:bg-secondary block px-4 py-2 transition-colors"
                        role="menuitem"
                      >
                        Register
                      </Link>
                    </>
                  )}
                </div>
              )}
            </div>

            {session?.user && (
              <Link
                href="/basket"
                className={`relative shrink-0 rounded-full p-1.5 transition-colors md:p-2 ${isActive("/basket") ? "bg-secondary" : "hover:bg-secondary"}`}
                aria-label="Shopping basket"
              >
                <ShoppingBag className="size-4 md:size-5" />
                {totalItems > 0 && (
                  <span
                    className="bg-primary text-primary-foreground absolute -top-0.5 -right-0.5 flex size-4 items-center justify-center rounded-full text-[10px] font-bold"
                    aria-live="polite"
                    aria-label="Number of items in basket"
                  >
                    {totalItems}
                  </span>
                )}
                <span className="sr-only">Basket</span>
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
          <form
            onSubmit={handleSearch}
            className="hover:bg-secondary relative flex w-full items-center rounded-full p-2 transition-colors"
            onMouseEnter={() => setIsSearchOpen(true)}
            onMouseLeave={() => setIsSearchOpen(false)}
            role="search"
            aria-label="Search books"
          >
            <Search
              className="text-foreground/60 pointer-events-none size-4 shrink-0"
              aria-hidden="true"
            />
            <input
              type="text"
              placeholder="Search books..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="w-full bg-transparent py-1 pl-2 text-sm outline-none"
              aria-label="Search books input"
            />
          </form>
        </div>
      </header>
    </>
  );
}
