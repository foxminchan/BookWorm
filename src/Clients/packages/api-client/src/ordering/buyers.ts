import type {
  Buyer,
  CreateBuyerRequest,
  ListBuyersQuery,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";
import ApiClient from "../client";

class BuyersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(query?: ListBuyersQuery): Promise<PagedResult<Buyer>> {
    const response = await this.client.get<Buyer[]>("/ordering/api/v1/buyers", {
      params: query,
    });

    return {
      items: response.data,
      totalCount: Number(response.headers["Pagination-Count"] || 0),
      link: response.headers["Link"],
    };
  }

  public async getCurrentBuyer(): Promise<Buyer> {
    const response = await this.client.get<Buyer>(`/ordering/api/v1/buyers/me`);
    return response.data;
  }

  public async get(id: string): Promise<Buyer> {
    const response = await this.client.get<Buyer>(
      `/ordering/api/v1/buyers/${id}`,
    );
    return response.data;
  }

  public async create(request: CreateBuyerRequest): Promise<Buyer> {
    const response = await this.client.post<Buyer>(
      "/ordering/api/v1/buyers",
      request,
    );
    return response.data;
  }

  public async updateAddress(request: UpdateAddressRequest): Promise<Buyer> {
    const response = await this.client.patch<Buyer>(
      `/ordering/api/v1/buyers/address`,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/ordering/api/v1/buyers/${id}`);
  }
}

export default new BuyersApiClient();
