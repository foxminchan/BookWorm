import ApiClient from "@/client";
import type {
  CreateBasketRequest,
  CustomerBasket,
  UpdateBasketRequest,
} from "@workspace/types/basket";

class BasketApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async get(): Promise<CustomerBasket> {
    const response = await this.client.get<CustomerBasket>(
      `/basket/api/v1/baskets`,
    );
    return response.data;
  }

  public async create(request: CreateBasketRequest): Promise<CustomerBasket> {
    const response = await this.client.post<CustomerBasket>(
      `/basket/api/v1/baskets`,
      request,
    );
    return response.data;
  }

  public async update(request: UpdateBasketRequest): Promise<CustomerBasket> {
    const response = await this.client.put<CustomerBasket>(
      `/basket/api/v1/baskets`,
      request,
    );
    return response.data;
  }

  public async delete(): Promise<void> {
    await this.client.delete<void>(`/basket/api/v1/baskets`);
  }
}

export default new BasketApiClient();
