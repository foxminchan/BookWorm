import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Order,
  OrderDetail,
  ListOrdersQuery,
} from "@workspace/types/ordering/orders";
import type { PagedResult } from "@workspace/types/shared";
import { v7 as uuidv7 } from "uuid";

export class OrdersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listOrders(query?: ListOrdersQuery): Promise<PagedResult<Order>> {
    return this.client.get<PagedResult<Order>>("/ordering/api/v1/orders", {
      params: query,
    });
  }

  async getOrder(id: string): Promise<OrderDetail> {
    return this.client.get<OrderDetail>(`/ordering/api/v1/orders/${id}`);
  }

  async createOrder(basketId: string): Promise<Order> {
    return this.client.post<Order>(
      "/ordering/api/v1/orders",
      { basketId },
      { headers: { "x-request-id": uuidv7() } },
    );
  }

  async cancelOrder(id: string): Promise<Order> {
    return this.client.patch<Order>(
      `/ordering/api/v1/orders/${id}/cancel`,
      undefined,
      {
        headers: { "x-request-id": uuidv7() },
      },
    );
  }

  async completeOrder(id: string): Promise<Order> {
    return this.client.patch<Order>(
      `/ordering/api/v1/orders/${id}/complete`,
      undefined,
      {
        headers: { "x-request-id": uuidv7() },
      },
    );
  }
}

export const ordersApiClient = new OrdersApiClient();
