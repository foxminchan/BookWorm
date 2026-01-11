import type {
  Book,
  CreateBookRequest,
  ListBooksQuery,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

import ApiClient from "../client";

class BooksApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = new ApiClient();
  }

  public async list(query?: ListBooksQuery): Promise<PagedResult<Book>> {
    const response = await this.client.get<PagedResult<Book>>(
      "/catalog/api/v1/books",
      {
        params: query,
      },
    );

    return response.data;
  }

  public async get(id: string): Promise<Book> {
    const response = await this.client.get<Book>(`/catalog/api/v1/books/${id}`);
    return response.data;
  }

  public async create(request: CreateBookRequest): Promise<Book> {
    const formData = new FormData();
    formData.append("name", request.name);
    formData.append("description", request.description);
    formData.append("price", request.price.toString());
    if (request.priceSale !== null && request.priceSale !== undefined) {
      formData.append("priceSale", request.priceSale.toString());
    }
    formData.append("categoryId", request.categoryId);
    formData.append("publisherId", request.publisherId);
    for (const authorId of request.authorIds) {
      formData.append("authorIds", authorId);
    }
    if (request.image) {
      formData.append("image", request.image);
    }

    const response = await this.client.post<Book>(
      "/catalog/api/v1/books",
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      },
    );
    return response.data;
  }

  public async update(request: UpdateBookRequest): Promise<Book> {
    const formData = new FormData();
    formData.append("id", request.id);
    formData.append("name", request.name);
    formData.append("description", request.description);
    formData.append("price", request.price.toString());
    if (request.priceSale !== null && request.priceSale !== undefined) {
      formData.append("priceSale", request.priceSale.toString());
    }
    formData.append("categoryId", request.categoryId);
    formData.append("publisherId", request.publisherId);
    for (const authorId of request.authorIds) {
      formData.append("authorIds", authorId);
    }
    if (request.image) {
      formData.append("image", request.image);
    }
    if (request.isRemoveImage) {
      formData.append("isRemoveImage", "true");
    }

    const response = await this.client.put<Book>(
      `/catalog/api/v1/books`,
      formData,
      {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      },
    );
    return response.data;
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(`/catalog/api/v1/books/${id}`);
  }
}

export default new BooksApiClient();
