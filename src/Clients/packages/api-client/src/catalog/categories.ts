import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";

import { apiClient } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class CategoriesApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(signal?: AbortSignal): Promise<Category[]> {
    const response = await this.client.get<Category[]>(
      API_ENDPOINTS.catalog.categories.root,
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async get(id: string, signal?: AbortSignal): Promise<Category> {
    const response = await this.client.get<Category>(
      API_ENDPOINTS.catalog.categories.byId(id),
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async create(request: CreateCategoryRequest): Promise<Category> {
    const response = await this.client.post<Category>(
      API_ENDPOINTS.catalog.categories.root,
      request,
    );
    return response.data;
  }

  public async update(request: UpdateCategoryRequest): Promise<Category> {
    const response = await this.client.put<Category>(
      API_ENDPOINTS.catalog.categories.root,
      request,
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.catalog.categories.byId(id));
  }
}

export default new CategoriesApiClient();
