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

export const canCompleteOrder = isOrderNew;
export const canCancelOrder = isOrderNew;
export const isOrderEditable = isOrderNew;
