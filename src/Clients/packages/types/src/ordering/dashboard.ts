import type { Order } from "./orders";

export type DailyOrders = { date: string; orders: number; revenue: number };
export type CategoryCount = { name: string; value: number };

export type Dashboard = {
  totalOrders: number;
  totalRevenue: number;
  totalCustomers: number;
  totalBooks: number;
  dailyOrders: DailyOrders[];
  categories: CategoryCount[];
  recentOrders: Order[];
  updatedAt: string;
};
