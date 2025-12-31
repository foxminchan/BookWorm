import type {
  Order,
  OrderDetail,
  ListOrdersQuery,
} from "@workspace/types/ordering/orders";
import type { PagedResult } from "@workspace/types/shared";
import { v7 as uuidv7 } from "uuid";
import ApiClient from "../client";

class OrdersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(query?: ListOrdersQuery): Promise<PagedResult<Order>> {
    const response = await this.client.get<Order[]>("/ordering/api/v1/orders", {
      params: query,
    });

    return {
      items: response.data,
      totalCount: Number(response.headers["Pagination-Count"] || 0),
      link: response.headers["Link"],
    };
  }

  public async get(id: string): Promise<OrderDetail> {
    const response = await this.client.get<OrderDetail>(
      `/ordering/api/v1/orders/${id}`,
    );
    return response.data;
  }

  public async create(basketId: string): Promise<Order> {
    const response = await this.client.post<Order>(
      "/ordering/api/v1/orders",
      { basketId },
      { headers: { "x-request-id": uuidv7() } },
    );
    return response.data;
  }

  public async cancel(id: string): Promise<Order> {
    const response = await this.client.patch<Order>(
      `/ordering/api/v1/orders/${id}/cancel`,
      undefined,
      {
        headers: { "x-request-id": uuidv7() },
      },
    );

    return response.data;
  }

  public async complete(id: string): Promise<Order> {
    const response = await this.client.patch<Order>(
      `/ordering/api/v1/orders/${id}/complete`,
      undefined,
      {
        headers: { "x-request-id": uuidv7() },
      },
    );

    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/ordering/api/v1/orders/${id}`);
  }
}

export default new OrdersApiClient();
