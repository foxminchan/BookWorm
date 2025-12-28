import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  BasketItem,
  CreateBasketRequest,
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";

export class BasketApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async getBasket(customerId: string): Promise<CustomerBasket> {
    return this.client.get<CustomerBasket>(`/${customerId}`);
  }

  async createBasket(request: CreateBasketRequest): Promise<CustomerBasket> {
    return this.client.post<CustomerBasket>("/", request);
  }

  async updateBasket(
    customerId: string,
    request: UpdateBasketRequest,
  ): Promise<CustomerBasket> {
    return this.client.put<CustomerBasket>(`/${customerId}`, request);
  }

  async deleteBasket(customerId: string): Promise<void> {
    return this.client.delete<void>(`/${customerId}`);
  }

  async addItem(customerId: string, item: BasketItem): Promise<CustomerBasket> {
    return this.client.post<CustomerBasket>(`/${customerId}/items`, item);
  }

  async updateItem(
    customerId: string,
    itemId: string,
    item: Partial<BasketItem>,
  ): Promise<CustomerBasket> {
    return this.client.put<CustomerBasket>(
      `/${customerId}/items/${itemId}`,
      item,
    );
  }

  async removeItem(
    customerId: string,
    itemId: string,
  ): Promise<CustomerBasket> {
    return this.client.delete<CustomerBasket>(`/${customerId}/items/${itemId}`);
  }
}

export const basketApiClient = new BasketApiClient();
