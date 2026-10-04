import { HttpResponse, http } from "msw";

import type { Dashboard } from "@workspace/types/ordering/dashboard";

import { booksStore } from "../catalog/books/data";
import { buyersStoreManager } from "./buyers/data";
import { ORDERING_API_BASE_URL } from "./constants";
import { ordersStoreManager } from "./orders/data";

function getDashboard(): Dashboard {
  const orders = ordersStoreManager.list();
  const books = booksStore.books;
  const now = new Date();
  const categories = new Map<string, number>();
  for (const book of books) {
    const name = book.category?.name ?? "Other";
    categories.set(name, (categories.get(name) ?? 0) + 1);
  }
  const dailyOrders = Array.from({ length: 7 }, (_, index) => {
    const day = new Date(now);
    day.setUTCDate(day.getUTCDate() - 6 + index);
    const date = day.toISOString().slice(0, 10);
    const daily = orders.filter(
      (order) => new Date(order.date).toISOString().slice(0, 10) === date,
    );
    return {
      date,
      orders: daily.length,
      revenue: daily
        .filter((order) => order.status === "Completed")
        .reduce((sum, order) => sum + order.total, 0),
    };
  });
  return {
    totalOrders: orders.length,
    totalRevenue: orders
      .filter((order) => order.status === "Completed")
      .reduce((sum, order) => sum + order.total, 0),
    totalCustomers: buyersStoreManager.list().length,
    totalBooks: books.length,
    categories: Array.from(categories, ([name, value]) => ({
      name,
      value,
    })).sort((a, b) => a.name.localeCompare(b.name)),
    dailyOrders,
    recentOrders: [...orders]
      .sort(
        (a, b) =>
          Date.parse(b.date) - Date.parse(a.date) || b.id.localeCompare(a.id),
      )
      .slice(0, 5)
      .map(({ id, date, total, status }) => ({ id, date, total, status })),
    updatedAt: now.toISOString(),
  };
}

export const dashboardHandlers = [
  http.get(`${ORDERING_API_BASE_URL}/api/v1/dashboard`, () =>
    HttpResponse.json(getDashboard()),
  ),
  http.get(
    `${ORDERING_API_BASE_URL}/api/v1/dashboard/stream`,
    ({ request }) => {
      let timer: ReturnType<typeof setInterval>;
      const encoder = new TextEncoder();
      const cleanup = () => {
        clearInterval(timer);
        request.signal.removeEventListener("abort", cleanup);
      };
      const stream = new ReadableStream<Uint8Array>({
        start(controller) {
          const send = () =>
            controller.enqueue(
              encoder.encode(
                `event: dashboard\ndata: ${JSON.stringify(getDashboard())}\n\n`,
              ),
            );
          send();
          timer = setInterval(send, 15_000);
          request.signal.addEventListener("abort", cleanup, { once: true });
        },
        cancel: cleanup,
      });
      return new HttpResponse(stream, {
        headers: { "Content-Type": "text/event-stream" },
      });
    },
  ),
];
