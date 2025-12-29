import ApiClient from "@/client";
import type {
  Author,
  CreateAuthorRequest,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";

class AuthorsApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(): Promise<Author[]> {
    const response = await this.client.get<Author[]>("/catalog/api/v1/authors");
    return response.data;
  }

  public async get(id: string): Promise<Author> {
    const response = await this.client.get<Author>(
      `/catalog/api/v1/authors/${id}`,
    );
    return response.data;
  }

  public async create(request: CreateAuthorRequest): Promise<Author> {
    const response = await this.client.post<Author>(
      "/catalog/api/v1/authors",
      request,
    );
    return response.data;
  }

  public async update(request: UpdateAuthorRequest): Promise<Author> {
    const response = await this.client.put<Author>(
      `/catalog/api/v1/authors`,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/catalog/api/v1/authors/${id}`);
  }
}

export default new AuthorsApiClient();
