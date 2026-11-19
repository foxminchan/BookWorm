import type { OrderStatus } from "@workspace/types/ordering/orders";

const ORDER_STATUS_COLORS: Record<OrderStatus, string> = {
  Completed:
    "bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400",
  Cancelled: "bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400",
  New: "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400",
};

const ORDER_STATUS_BORDERED_COLORS: Record<OrderStatus, string> = {
  Completed:
    "bg-green-50 text-green-900 border border-green-200 dark:bg-green-950/20 dark:text-green-300 dark:border-green-800",
  Cancelled:
    "bg-red-50 text-red-900 border border-red-200 dark:bg-red-950/20 dark:text-red-300 dark:border-red-800",
  New: "bg-blue-50 text-blue-900 border border-blue-200 dark:bg-blue-950/20 dark:text-blue-300 dark:border-blue-800",
};

const ORDER_STATUS_STYLES: Record<OrderStatus, string> = {
  New: "bg-blue-100 text-blue-800",
  Completed: "bg-green-100 text-green-800",
  Cancelled: "bg-red-100 text-red-800",
};

const FALLBACK_STATUS_COLOR = "bg-gray-100 text-gray-800";

export function getOrderStatusColor(status: OrderStatus): string {
  return ORDER_STATUS_COLORS[status] ?? FALLBACK_STATUS_COLOR;
}

export function getOrderStatusColorBordered(status: OrderStatus): string {
  return ORDER_STATUS_BORDERED_COLORS[status] ?? FALLBACK_STATUS_COLOR;
}

export function getOrderStatusStyle(status: OrderStatus): string {
  return ORDER_STATUS_STYLES[status] ?? FALLBACK_STATUS_COLOR;
}

export function isOrderNew(status: OrderStatus): boolean {
  return status === "New";
}

export function canCompleteOrder(status: OrderStatus): boolean {
  return isOrderNew(status);
}

export function canCancelOrder(status: OrderStatus): boolean {
  return isOrderNew(status);
}

export function isOrderEditable(status: OrderStatus): boolean {
  return isOrderNew(status);
}
