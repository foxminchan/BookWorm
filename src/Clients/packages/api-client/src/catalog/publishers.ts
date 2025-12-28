import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Publisher,
  CreatePublisherRequest,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import type { PagedResult } from "@workspace/types/shared";

export class PublishersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listPublishers(params?: {
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<Publisher>> {
    return this.client.get<PagedResult<Publisher>>(
      "/catalog/api/v1/publishers",
      { params },
    );
  }

  async getPublisher(id: string): Promise<Publisher> {
    return this.client.get<Publisher>(`/catalog/api/v1/publishers/${id}`);
  }

  async createPublisher(request: CreatePublisherRequest): Promise<Publisher> {
    return this.client.post<Publisher>("/catalog/api/v1/publishers", request);
  }

  async updatePublisher(
    id: string,
    request: UpdatePublisherRequest,
  ): Promise<Publisher> {
    return this.client.put<Publisher>(
      `/catalog/api/v1/publishers/${id}`,
      request,
    );
  }

  async deletePublisher(id: string): Promise<void> {
    return this.client.delete<void>(`/catalog/api/v1/publishers/${id}`);
  }
}

export const publishersApiClient = new PublishersApiClient();
