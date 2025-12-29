import { http, HttpResponse } from "msw";
import type { Order } from "@workspace/types/ordering/orders";
import { ordersStoreManager } from "./data";
import { buildPaginationLinks } from "@workspace/utils/link";
import { ORDERING_API_BASE_URL } from "@/ordering/constants";

export const ordersHandlers = [
  http.get(`${ORDERING_API_BASE_URL}/api/v1/orders/:id`, ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const order = ordersStoreManager.get(id);

    if (!order) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(order, { status: 200 });
  }),

  http.get(`${ORDERING_API_BASE_URL}/api/v1/orders`, ({ request }) => {
    const url = new URL(request.url);
    const pageIndex = Number.parseInt(url.searchParams.get("pageIndex") || "1");
    const pageSize = Number.parseInt(url.searchParams.get("pageSize") || "10");
    const statusFilter = url.searchParams.get("status");

    let allOrders = ordersStoreManager.list();

    if (statusFilter) {
      allOrders = allOrders.filter((o) => o.status === statusFilter);
    }

    const totalCount = allOrders.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageIndex - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const items = allOrders.slice(startIndex, endIndex);

    const orderList: Order[] = items.map((o) => ({
      id: o.id,
      date: o.date,
      total: o.total,
      status: o.status,
    }));

    const hasPreviousPage = pageIndex > 1;
    const hasNextPage = pageIndex < totalPages;

    const links = buildPaginationLinks(
      request.url,
      pageIndex,
      pageSize,
      totalPages,
      hasPreviousPage,
      hasNextPage,
    );

    const headers = new Headers();
    headers.set("Link", links.join(","));
    headers.set("Pagination-Count", totalCount.toString());

    return HttpResponse.json(orderList, { status: 200, headers });
  }),

  http.post(`${ORDERING_API_BASE_URL}/api/v1/orders`, async () => {
    const order = ordersStoreManager.create([
      { id: "mock-item", quantity: 1, price: 29.99, name: "Mock Book" },
    ]);

    const headers = new Headers();
    headers.set(
      "Location",
      `${ORDERING_API_BASE_URL}/api/v1/orders/${order.id}`,
    );

    return HttpResponse.json(order.id, { status: 201, headers });
  }),

  http.patch(
    `${ORDERING_API_BASE_URL}/api/v1/orders/:orderId/cancel`,
    ({ params }) => {
      const { orderId } = params;

      if (!orderId || typeof orderId !== "string") {
        return new HttpResponse(null, { status: 400 });
      }

      const order = ordersStoreManager.updateStatus(orderId, "Cancelled");

      if (!order) {
        return new HttpResponse(null, { status: 404 });
      }

      return HttpResponse.json(order, { status: 200 });
    },
  ),

  http.patch(
    `${ORDERING_API_BASE_URL}/api/v1/orders/:orderId/complete`,
    ({ params }) => {
      const { orderId } = params;

      if (!orderId || typeof orderId !== "string") {
        return new HttpResponse(null, { status: 400 });
      }

      const order = ordersStoreManager.updateStatus(orderId, "Completed");

      if (!order) {
        return new HttpResponse(null, { status: 404 });
      }

      return HttpResponse.json(order, { status: 200 });
    },
  ),

  http.delete(`${ORDERING_API_BASE_URL}/api/v1/orders/:id`, ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const deleted = ordersStoreManager.delete(id);

    if (!deleted) {
      return new HttpResponse(null, { status: 404 });
    }

    return new HttpResponse(null, { status: 204 });
  }),
];
