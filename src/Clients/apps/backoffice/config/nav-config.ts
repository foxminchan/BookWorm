import {
  BookOpen,
  LayoutDashboard,
  type LucideIcon,
  Package,
  PenTool,
  ShoppingCart,
  Star,
  Tag,
  UsersIcon,
} from "lucide-react";

export type BackofficeNavItem = {
  readonly href: string;
  readonly icon: LucideIcon;
  readonly label: string;
};

export type BackofficeNavGroup = {
  readonly title: string | null;
  readonly items: readonly BackofficeNavItem[];
};

/**
 * Centralized navigation structure for the Backoffice sidebar.
 */
export const BACKOFFICE_NAV_GROUPS: readonly BackofficeNavGroup[] = [
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
