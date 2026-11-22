import type {
  Buyer,
  CreateBuyerRequest,
  ListBuyersQuery,
  UpdateAddressRequest,
} from "@workspace/types/ordering/buyers";
import type { PagedResult } from "@workspace/types/shared";

import { HttpHeaders, apiClient, getHeaderValue } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class BuyersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(
    query?: ListBuyersQuery,
    signal?: AbortSignal,
  ): Promise<PagedResult<Buyer>> {
    const response = await this.client.get<Buyer[]>(
      API_ENDPOINTS.ordering.buyers.root,
      {
        params: query,
        signal,
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

  public async getCurrentBuyer(signal?: AbortSignal): Promise<Buyer> {
    const response = await this.client.get<Buyer>(
      API_ENDPOINTS.ordering.buyers.me,
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async get(id: string, signal?: AbortSignal): Promise<Buyer> {
    const response = await this.client.get<Buyer>(
      API_ENDPOINTS.ordering.buyers.byId(id),
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async create(request: CreateBuyerRequest): Promise<Buyer> {
    const response = await this.client.post<Buyer>(
      API_ENDPOINTS.ordering.buyers.root,
      request,
      { headers: { [HttpHeaders.RequestId]: crypto.randomUUID() } },
    );
    return response.data;
  }

  public async updateAddress(request: UpdateAddressRequest): Promise<Buyer> {
    const response = await this.client.patch<Buyer>(
      API_ENDPOINTS.ordering.buyers.address,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.ordering.buyers.byId(id));
  }
}

export default new BuyersApiClient();
