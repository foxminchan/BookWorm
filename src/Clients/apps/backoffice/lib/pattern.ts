import { match } from "ts-pattern";

export type OrderStatus = "New" | "Completed" | "Cancelled";

export function getOrderStatusStyle(status: OrderStatus): string {
  return match(status)
    .with("New", () => "bg-blue-100 text-blue-800")
    .with("Completed", () => "bg-green-100 text-green-800")
    .with("Cancelled", () => "bg-red-100 text-red-800")
    .exhaustive();
}

export function isOrderNew(status: OrderStatus): boolean {
  return match(status)
    .with("New", () => true)
    .otherwise(() => false);
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
