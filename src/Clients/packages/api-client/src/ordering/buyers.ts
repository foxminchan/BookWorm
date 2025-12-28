import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Buyer,
  CreateBuyerRequest,
  ListBuyersQuery,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";

export class BuyersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listBuyers(query?: ListBuyersQuery): Promise<PagedResult<Buyer>> {
    return this.client.get<PagedResult<Buyer>>("/ordering/api/v1/buyers", {
      params: query,
    });
  }

  async getBuyer(id: string): Promise<Buyer> {
    return this.client.get<Buyer>(`/ordering/api/v1/buyers/${id}`);
  }

  async createBuyer(request: CreateBuyerRequest): Promise<Buyer> {
    return this.client.post<Buyer>("/ordering/api/v1/buyers", request);
  }

  async updateAddress(
    id: string,
    request: UpdateAddressRequest,
  ): Promise<Buyer> {
    return this.client.patch<Buyer>(
      `/ordering/api/v1/buyers/${id}/address`,
      request,
    );
  }

  async deleteBuyer(id: string): Promise<void> {
    return this.client.delete<void>(`/ordering/api/v1/buyers/${id}`);
  }
}

export const buyersApiClient = new BuyersApiClient();
