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
