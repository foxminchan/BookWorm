"use client";

import type React from "react";

import { useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { ShoppingBag, User, Home, Search } from "lucide-react";
import { useRouter, usePathname } from "next/navigation";
import { useAtomValue } from "jotai";
import { basketItemsAtom } from "@/atoms/basket-atom";
import { Button } from "@workspace/ui/components/button";

export function Header() {
  const router = useRouter();
  const pathname = usePathname();
  const items = useAtomValue(basketItemsAtom);
  const totalItems = items.length;
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

  const handleLogout = () => {
    // TODO: Implement actual logout logic
    console.log("Logout clicked");
    setIsAccountOpen(false);
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
        className="hidden md:block sticky top-0 z-50 w-full border-b bg-background/80 backdrop-blur-md"
        role="banner"
      >
        <div className="container mx-auto flex h-20 items-center px-4 gap-4">
          {/* Left: Logo */}
          <Link
            href="/"
            className="flex items-center gap-1 md:gap-2 text-lg md:text-2xl font-bold tracking-tight shrink-0 text-foreground"
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
            className="hidden md:flex flex-1 justify-center items-center gap-8 text-sm font-medium"
            aria-label="Main Navigation"
          >
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className={`transition-colors pb-1 border-b-2 ${
                  isActive(link.href)
                    ? "text-primary border-primary"
                    : "text-foreground hover:text-primary border-b-2 border-transparent"
                }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>

          <div className="flex items-center gap-2 md:gap-5 ml-auto">
            <div
              className="relative"
              onMouseEnter={handleSearchMouseEnter}
              onMouseLeave={handleSearchMouseLeave}
            >
              <form onSubmit={handleSearch}>
                <button
                  type="button"
                  className="p-1.5 md:p-2 hover:bg-secondary rounded-full transition-colors shrink-0"
                  onClick={() => setIsSearchOpen(!isSearchOpen)}
                  aria-label="Toggle search"
                >
                  <Search className="size-4 md:size-5 text-foreground/60" />
                </button>
                {isSearchOpen && (
                  <div className="absolute right-0 top-full mt-2 bg-background border border-foreground/10 rounded-lg p-2 shadow-lg w-48 md:w-64 animate-in fade-in slide-in-from-top-2 duration-200">
                    <input
                      type="text"
                      placeholder="Search books..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      autoFocus
                      className="w-full px-3 py-2 bg-transparent outline-none border-b border-foreground/20 focus:border-primary placeholder:text-foreground/40 text-sm"
                      aria-label="Search books"
                    />
                  </div>
                )}
              </form>
            </div>

            <Link
              href="/"
              className={`p-1.5 md:p-2 rounded-full transition-colors shrink-0 ${isActive("/") ? "bg-secondary" : "hover:bg-secondary"}`}
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
              <Link
                href="/account"
                className={`p-1.5 md:p-2 rounded-full transition-colors shrink-0 block ${isActive("/account") ? "bg-secondary" : "hover:bg-secondary"}`}
                aria-label="Account"
              >
                <User className="size-4 md:size-5" />
                <span className="sr-only">Account</span>
              </Link>

              {/* Dropdown Menu */}
              {isAccountOpen && (
                <div
                  className="absolute right-0 mt-2 w-40 md:w-48 bg-background border border-foreground/10 rounded-lg shadow-lg py-2 text-sm animate-in fade-in slide-in-from-top-2 duration-200"
                  role="menu"
                  aria-label="Account menu"
                >
                  <Link
                    href="/account"
                    className="block px-4 py-2 hover:bg-secondary transition-colors"
                    role="menuitem"
                  >
                    My Profile
                  </Link>
                  <Link
                    href="/account/orders"
                    className="block px-4 py-2 hover:bg-secondary transition-colors"
                    role="menuitem"
                  >
                    Order History
                  </Link>
                  <Button
                    variant="ghost"
                    onClick={handleLogout}
                    className="w-full justify-start px-4 py-2 h-auto text-primary font-normal"
                    role="menuitem"
                  >
                    Logout
                  </Button>
                </div>
              )}
            </div>

            <Link
              href="/basket"
              className={`p-1.5 md:p-2 rounded-full transition-colors relative shrink-0 ${isActive("/basket") ? "bg-secondary" : "hover:bg-secondary"}`}
              aria-label="Shopping basket"
            >
              <ShoppingBag className="size-4 md:size-5" />
              {totalItems > 0 && (
                <span
                  className="absolute -top-0.5 -right-0.5 size-4 bg-primary text-[10px] font-bold text-primary-foreground rounded-full flex items-center justify-center"
                  aria-live="polite"
                  aria-label="Number of items in basket"
                >
                  {totalItems}
                </span>
              )}
              <span className="sr-only">Basket</span>
            </Link>
          </div>
        </div>
      </header>
      <header
        className="md:hidden sticky top-0 z-50 w-full border-b bg-background/80 backdrop-blur-md"
        role="banner"
      >
        <div className="container mx-auto flex h-16 items-center px-3 gap-2">
          {/* Search Form */}
          <form
            onSubmit={handleSearch}
            className="w-full relative flex items-center p-2 hover:bg-secondary rounded-full transition-colors"
            onMouseEnter={() => setIsSearchOpen(true)}
            onMouseLeave={() => setIsSearchOpen(false)}
            role="search"
            aria-label="Search books"
          >
            <Search
              className="size-4 text-foreground/60 pointer-events-none shrink-0"
              aria-hidden="true"
            />
            <input
              type="text"
              placeholder="Search books..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-2 py-1 bg-transparent outline-none text-sm w-full"
              aria-label="Search books input"
            />
          </form>
        </div>
      </header>
    </>
  );
}
