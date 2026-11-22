import {
  Home,
  type LucideIcon,
  ShoppingBag,
  ShoppingCart,
  User,
} from "lucide-react";

export type NavLink = {
  readonly href: string;
  readonly label: string;
};

export type MobileNavItem = NavLink & {
  readonly icon: LucideIcon;
};

export const STOREFRONT_HEADER_NAV: readonly NavLink[] = [
  { href: "/shop", label: "Shop" },
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
  { href: "/about", label: "Our Story" },
] as const;

export const STOREFRONT_FOOTER_QUICK_LINKS: readonly NavLink[] = [
  { href: "/shop", label: "Books" },
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
] as const;

export const STOREFRONT_FOOTER_HELP_LINKS: readonly NavLink[] = [
  { href: "/shipping", label: "Shipping Info" },
  { href: "/returns", label: "Returns" },
  { href: "/about", label: "Our Story" },
] as const;

export const STOREFRONT_MOBILE_BASE_NAV: readonly MobileNavItem[] = [
  { href: "/", icon: Home, label: "Home" },
  { href: "/shop", icon: ShoppingBag, label: "Shop" },
] as const;

export const STOREFRONT_MOBILE_BASKET_NAV: MobileNavItem = {
  href: "/basket",
  icon: ShoppingCart,
  label: "Basket",
} as const;

export const STOREFRONT_MOBILE_ACCOUNT_NAV: MobileNavItem = {
  href: "/account",
  icon: User,
  label: "Account",
} as const;

export const STOREFRONT_MOBILE_MORE_LINKS: readonly NavLink[] = [
  { href: "/categories", label: "Categories" },
  { href: "/publishers", label: "Publishers" },
  { href: "/about", label: "Our Story" },
] as const;

export const STOREFRONT_ACCOUNT_AUTH_NAV: readonly NavLink[] = [
  { href: "/account", label: "My Profile" },
  { href: "/account/orders", label: "Order History" },
] as const;

export const STOREFRONT_ACCOUNT_GUEST_NAV: readonly NavLink[] = [
  { href: "/login", label: "Login" },
  { href: "/register", label: "Register" },
] as const;

export const STOREFRONT_ACCOUNT_SECTIONS: readonly NavLink[] = [
  { href: "/account/orders", label: "Order History" },
] as const;
