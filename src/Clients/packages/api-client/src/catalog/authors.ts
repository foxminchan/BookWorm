import type {
  Author,
  CreateAuthorRequest,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";

import { apiClient } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class AuthorsApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(): Promise<Author[]> {
    const response = await this.client.get<Author[]>(
      API_ENDPOINTS.catalog.authors.root,
    );
    return response.data;
  }

  public async get(id: string): Promise<Author> {
    const response = await this.client.get<Author>(
      API_ENDPOINTS.catalog.authors.byId(id),
    );
    return response.data;
  }

  public async create(request: CreateAuthorRequest): Promise<Author> {
    const response = await this.client.post<Author>(
      API_ENDPOINTS.catalog.authors.root,
      request,
    );
    return response.data;
  }

  public async update(request: UpdateAuthorRequest): Promise<Author> {
    const response = await this.client.put<Author>(
      API_ENDPOINTS.catalog.authors.root,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.catalog.authors.byId(id));
  }
}

export default new AuthorsApiClient();
