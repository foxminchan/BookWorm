import { HttpResponse, http } from "msw";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import { HttpHeaders } from "@workspace/api-client/headers";
import type {
  Order,
  OrderEventHistory,
  OrderSummarySnapshot,
} from "@workspace/types/ordering/orders";
import { buildPaginationLinks } from "@workspace/utils/link";

import {
  createCorsHeaders,
  createPreflightResponse,
  mockUrl,
} from "../../helpers/index";
import { ordersStoreManager } from "./data";

export const ordersHandlers = [
  http.options(
    mockUrl(API_ENDPOINTS.ordering.orders.summary()),
    ({ request }) => createPreflightResponse(request),
  ),

  http.get(mockUrl(API_ENDPOINTS.ordering.orders.history()), ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const order = ordersStoreManager.get(id);

    if (!order) {
      return new HttpResponse(null, { status: 404 });
    }

    const history: OrderEventHistory[] = [
      {
        sequence: 1,
        version: 1,
        eventType: "order_placed",
        timestamp: order.date,
        status: "New",
        totalPrice: order.total,
      },
    ];

    if (order.status !== "New") {
      history.push({
        sequence: 2,
        version: 2,
        eventType:
          order.status === "Completed" ? "order_completed" : "order_cancelled",
        timestamp: order.date,
        status: order.status,
        totalPrice: order.total,
      });
    }

    return HttpResponse.json(history, { status: 200 });
  }),

  http.get(
    mockUrl(API_ENDPOINTS.ordering.orders.summary()),
    ({ params, request }) => {
      const { id } = params;

      if (!id || typeof id !== "string") {
        return new HttpResponse(null, { status: 400 });
      }

      const order = ordersStoreManager.get(id);

      if (!order) {
        return new HttpResponse(null, { status: 404 });
      }

      const url = new URL(request.url);
      const requestedVersion = url.searchParams.get("version");
      const defaultVersion = order.status === "New" ? 1 : 2;
      const version = requestedVersion
        ? Number.parseInt(requestedVersion, 10)
        : defaultVersion;

      const etag = `"${version}"`;
      const ifNoneMatch = request.headers.get(HttpHeaders.IfNoneMatch);

      if (
        ifNoneMatch
          ?.split(",")
          .map((tag) => tag.trim().replace(/^W\//, ""))
          .some((tag) => tag === "*" || tag === etag)
      ) {
        return new HttpResponse(null, {
          status: 304,
          headers: createCorsHeaders(request, { [HttpHeaders.ETag]: etag }),
        });
      }

      const summary: OrderSummarySnapshot = {
        id: order.id,
        status: version === 1 ? "New" : order.status,
        totalPrice: order.total,
        version,
      };

      return HttpResponse.json(summary, {
        status: 200,
        headers: createCorsHeaders(request, { [HttpHeaders.ETag]: etag }),
      });
    },
  ),

  http.get(mockUrl(API_ENDPOINTS.ordering.orders.byId()), ({ params }) => {
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

  http.get(mockUrl(API_ENDPOINTS.ordering.orders.root), ({ request }) => {
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
    headers.set(HttpHeaders.Link, links.join(","));
    headers.set(HttpHeaders.PaginationCount, totalCount.toString());

    return HttpResponse.json(orderList, { status: 200, headers });
  }),

  http.post(mockUrl(API_ENDPOINTS.ordering.orders.root), () => {
    const order = ordersStoreManager.create([
      { id: "mock-item", quantity: 1, price: 29.99, name: "Mock Book" },
    ]);

    const headers = new Headers();
    headers.set(
      HttpHeaders.Location,
      mockUrl(API_ENDPOINTS.ordering.orders.byId(order.id)),
    );

    return HttpResponse.json(order.id, { status: 201, headers });
  }),

  http.patch(mockUrl(API_ENDPOINTS.ordering.orders.cancel()), ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const order = ordersStoreManager.updateStatus(id, "Cancelled");

    if (!order) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(order, { status: 200 });
  }),

  http.patch(
    mockUrl(API_ENDPOINTS.ordering.orders.complete()),
    ({ params }) => {
      const { id } = params;

      if (!id || typeof id !== "string") {
        return new HttpResponse(null, { status: 400 });
      }

      const order = ordersStoreManager.updateStatus(id, "Completed");

      if (!order) {
        return new HttpResponse(null, { status: 404 });
      }

      return HttpResponse.json(order, { status: 200 });
    },
  ),

  http.delete(mockUrl(API_ENDPOINTS.ordering.orders.byId()), ({ params }) => {
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
