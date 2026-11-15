export type OrderStatus = "New" | "Cancelled" | "Completed";

export type Order = {
  id: string;
  date: string;
  total: number;
  status: OrderStatus;
};

export type OrderItem = {
  id: string;
  quantity: number;
  price: number;
  name?: string | null;
};

export type OrderDetail = {
  id: string;
  date: string;
  total: number;
  status: OrderStatus;
  items: OrderItem[];
};

export type ListOrdersQuery = {
  pageIndex?: number;
  pageSize?: number;
  status?: OrderStatus;
  buyerId?: string;
};

export type OrderEventHistory = {
  sequence: number;
  version: number;
  eventType: string;
  timestamp: string;
  status?: OrderStatus | null;
  totalPrice?: number | null;
};

export type OrderSummarySnapshot = {
  id: string;
  status: OrderStatus;
  totalPrice: number;
  version: number;
};

export type OrderSummaryQuery = {
  version?: number;
  at?: string;
};
