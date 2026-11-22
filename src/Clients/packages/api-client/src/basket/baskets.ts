import type {
  CreateBasketRequest,
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";

import { apiClient } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class BasketApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async get(signal?: AbortSignal): Promise<CustomerBasket> {
    const response = await this.client.get<CustomerBasket>(
      API_ENDPOINTS.basket.root,
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async create(request: CreateBasketRequest): Promise<CustomerBasket> {
    const response = await this.client.post<CustomerBasket>(
      API_ENDPOINTS.basket.root,
      request,
    );
    return response.data;
  }

  public async update(request: UpdateBasketRequest): Promise<CustomerBasket> {
    const response = await this.client.put<CustomerBasket>(
      API_ENDPOINTS.basket.root,
      request,
    );
    return response.data;
  }

  public async delete(): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.basket.root);
  }
}

export default new BasketApiClient();
