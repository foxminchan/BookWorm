import { ApiClient } from "../client";
import axiosConfig from "../config";
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import type { PagedResult } from "@workspace/types/shared";

export class CategoriesApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient(axiosConfig);
  }

  async listCategories(params?: {
    pageIndex?: number;
    pageSize?: number;
  }): Promise<PagedResult<Category>> {
    return this.client.get<PagedResult<Category>>(
      "/catalog/api/v1/categories",
      { params },
    );
  }

  async getCategory(id: string): Promise<Category> {
    return this.client.get<Category>(`/catalog/api/v1/categories/${id}`);
  }

  async createCategory(request: CreateCategoryRequest): Promise<Category> {
    return this.client.post<Category>("/catalog/api/v1/categories", request);
  }

  async updateCategory(
    id: string,
    request: UpdateCategoryRequest,
  ): Promise<Category> {
    return this.client.put<Category>(
      `/catalog/api/v1/categories/${id}`,
      request,
    );
  }

  async deleteCategory(id: string): Promise<void> {
    return this.client.delete<void>(`/catalog/api/v1/categories/${id}`);
  }
}

export const categoriesApiClient = new CategoriesApiClient();
