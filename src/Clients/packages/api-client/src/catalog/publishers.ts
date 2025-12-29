import ApiClient from "../client";
import type {
  Publisher,
  CreatePublisherRequest,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";

class PublishersApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(): Promise<Publisher[]> {
    const response = await this.client.get<Publisher[]>(
      "/catalog/api/v1/publishers",
    );
    return response.data;
  }

  public async get(id: string): Promise<Publisher> {
    const response = await this.client.get<Publisher>(
      `/catalog/api/v1/publishers/${id}`,
    );
    return response.data;
  }

  public async create(request: CreatePublisherRequest): Promise<Publisher> {
    const response = await this.client.post<Publisher>(
      "/catalog/api/v1/publishers",
      request,
    );
    return response.data;
  }

  public async update(request: UpdatePublisherRequest): Promise<Publisher> {
    const response = await this.client.put<Publisher>(
      `/catalog/api/v1/publishers`,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/catalog/api/v1/publishers/${id}`);
  }
}

export default new PublishersApiClient();
