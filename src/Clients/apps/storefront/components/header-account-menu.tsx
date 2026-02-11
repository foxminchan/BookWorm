"use client";

import Link from "next/link";

import { User } from "lucide-react";

import { Button } from "@workspace/ui/components/button";

import { useDelayedToggle } from "@/hooks/useDelayedToggle";
import { useLogout } from "@/hooks/useLogout";
import { useSession } from "@/lib/auth-client";

const AUTHENTICATED_MENU_ITEMS = [
  { href: "/account", label: "My Profile" },
  { href: "/account/orders", label: "Order History" },
] as const;

const GUEST_MENU_ITEMS = [
  { href: "/login", label: "Login" },
  { href: "/register", label: "Register" },
] as const;

type HeaderAccountMenuProps = {
  readonly isAccountActive: boolean;
};

export function HeaderAccountMenu({ isAccountActive }: HeaderAccountMenuProps) {
  const { data: session } = useSession();
  const { logout } = useLogout();
  const { isOpen, close, handleMouseEnter, handleMouseLeave } =
    useDelayedToggle({ closeDelay: 200 });

  const handleLogout = async () => {
    close();
    await logout();
  };

  return (
    <nav
      className="relative"
      aria-label="Account"
      onMouseEnter={handleMouseEnter}
      onMouseLeave={handleMouseLeave}
    >
      <Button
        type="button"
        variant="ghost"
        size="icon"
        className={`h-9 w-9 rounded-full ${
          isAccountActive ? "bg-secondary" : ""
        }`}
        onKeyDown={(e) => {
          if (e.key === "Escape" && isOpen) {
            close();
          }
        }}
        aria-label="Account menu"
        aria-expanded={isOpen}
        aria-haspopup="true"
      >
        <User className="size-4 md:size-5" aria-hidden="true" />
        <span className="sr-only">Account</span>
      </Button>

      {isOpen && (
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
            GUEST_MENU_ITEMS.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="hover:bg-secondary block px-4 py-2 transition-colors"
                role="menuitem"
              >
                {item.label}
              </Link>
            ))
          )}
        </div>
      )}
    </nav>
  );
}
