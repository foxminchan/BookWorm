import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Author,
  CreateAuthorRequest,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";
import type { PagedResult } from "@workspace/types/shared";

export class AuthorsApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listAuthors(params?: {
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<Author>> {
    return this.client.get<PagedResult<Author>>("/catalog/api/v1/authors", {
      params,
    });
  }

  async getAuthor(id: string): Promise<Author> {
    return this.client.get<Author>(`/catalog/api/v1/authors/${id}`);
  }

  async createAuthor(request: CreateAuthorRequest): Promise<Author> {
    return this.client.post<Author>("/catalog/api/v1/authors", request);
  }

  async updateAuthor(
    id: string,
    request: UpdateAuthorRequest,
  ): Promise<Author> {
    return this.client.put<Author>(`/catalog/api/v1/authors/${id}`, request);
  }

  async deleteAuthor(id: string): Promise<void> {
    return this.client.delete<void>(`/catalog/api/v1/authors/${id}`);
  }
}

export const authorsApiClient = new AuthorsApiClient();
