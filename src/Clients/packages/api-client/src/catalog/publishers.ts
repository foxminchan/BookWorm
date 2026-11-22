import type {
  CreatePublisherRequest,
  Publisher,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";

import { apiClient } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class PublishersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(signal?: AbortSignal): Promise<Publisher[]> {
    const response = await this.client.get<Publisher[]>(
      API_ENDPOINTS.catalog.publishers.root,
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async get(id: string, signal?: AbortSignal): Promise<Publisher> {
    const response = await this.client.get<Publisher>(
      API_ENDPOINTS.catalog.publishers.byId(id),
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async create(request: CreatePublisherRequest): Promise<Publisher> {
    const response = await this.client.post<Publisher>(
      API_ENDPOINTS.catalog.publishers.root,
      request,
    );
    return response.data;
  }

  public async update(request: UpdatePublisherRequest): Promise<Publisher> {
    const response = await this.client.put<Publisher>(
      API_ENDPOINTS.catalog.publishers.root,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.catalog.publishers.byId(id));
  }
}

export default new PublishersApiClient();
