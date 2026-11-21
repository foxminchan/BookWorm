import type {
  ListOrdersQuery,
  Order,
  OrderDetail,
  OrderEventHistory,
  OrderSummaryQuery,
  OrderSummarySnapshot,
} from "@workspace/types/ordering/orders";
import type { PagedResult } from "@workspace/types/shared";

import {
  type ConditionalResponse,
  HttpHeaders,
  apiClient,
  getHeaderValue,
} from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class OrdersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(query?: ListOrdersQuery): Promise<PagedResult<Order>> {
    const response = await this.client.get<Order[]>(
      API_ENDPOINTS.ordering.orders.root,
      {
        params: query,
      },
    );

    return {
      items: response.data,
      totalCount: Number(
        getHeaderValue(response.headers, HttpHeaders.PaginationCount) || 0,
      ),
      link: getHeaderValue(response.headers, HttpHeaders.Link),
    };
  }

  public async get(id: string): Promise<OrderDetail> {
    const response = await this.client.get<OrderDetail>(
      API_ENDPOINTS.ordering.orders.byId(id),
    );
    return response.data;
  }

  public async getHistory(id: string): Promise<OrderEventHistory[]> {
    const response = await this.client.get<OrderEventHistory[]>(
      API_ENDPOINTS.ordering.orders.history(id),
    );
    return response.data;
  }

  public async getSummary(
    id: string,
    query?: OrderSummaryQuery,
  ): Promise<OrderSummarySnapshot> {
    const response = await this.client.get<OrderSummarySnapshot>(
      API_ENDPOINTS.ordering.orders.summary(id),
      { params: query },
    );
    return response.data;
  }

  public async getSummaryConditional(
    id: string,
    query?: OrderSummaryQuery,
    options?: { ifNoneMatch?: string },
  ): Promise<ConditionalResponse<OrderSummarySnapshot>> {
    return this.client.getConditional<OrderSummarySnapshot>(
      API_ENDPOINTS.ordering.orders.summary(id),
      {
        params: query,
        ifNoneMatch: options?.ifNoneMatch,
      },
    );
  }

  public async create(basketId: string): Promise<string> {
    const response = await this.client.post<string>(
      API_ENDPOINTS.ordering.orders.root,
      { basketId },
      { headers: { [HttpHeaders.RequestId]: crypto.randomUUID() } },
    );
    return response.data;
  }

  public async cancel(id: string): Promise<Order> {
    const response = await this.client.patch<Order>(
      API_ENDPOINTS.ordering.orders.cancel(id),
      undefined,
      {
        headers: { [HttpHeaders.RequestId]: crypto.randomUUID() },
      },
    );

    return response.data;
  }

  public async complete(id: string): Promise<Order> {
    const response = await this.client.patch<Order>(
      API_ENDPOINTS.ordering.orders.complete(id),
      undefined,
      {
        headers: { [HttpHeaders.RequestId]: crypto.randomUUID() },
      },
    );

    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.ordering.orders.byId(id));
  }
}

export default new OrdersApiClient();
