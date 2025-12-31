import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import ApiClient from "../client";

class CategoriesApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(): Promise<Category[]> {
    const response = await this.client.get<Category[]>(
      "/catalog/api/v1/categories",
    );
    return response.data;
  }

  public async get(id: string): Promise<Category> {
    const response = await this.client.get<Category>(
      `/catalog/api/v1/categories/${id}`,
    );
    return response.data;
  }

  public async create(request: CreateCategoryRequest): Promise<Category> {
    const response = await this.client.post<Category>(
      "/catalog/api/v1/categories",
      request,
    );
    return response.data;
  }

  public async update(request: UpdateCategoryRequest): Promise<Category> {
    const response = await this.client.put<Category>(
      `/catalog/api/v1/categories`,
      request,
    );
    return response.data;
  }

  public async deleteCategory(id: string): Promise<void> {
    await this.client.delete<void>(`/catalog/api/v1/categories/${id}`);
  }
}

export default new CategoriesApiClient();
