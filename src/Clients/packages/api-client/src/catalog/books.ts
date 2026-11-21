import { AxiosHeaders } from "axios";

import type {
  Book,
  CreateBookRequest,
  ListBooksQuery,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import type { PagedResult } from "@workspace/types/shared";

import { type ConditionalResponse, HttpHeaders, apiClient } from "../client";
import type ApiClient from "../client";
import { API_ENDPOINTS } from "../endpoints";

class BooksApiClient {
  private readonly client: ApiClient;

  constructor() {
    this.client = apiClient;
  }

  public async list(
    query?: ListBooksQuery,
    signal?: AbortSignal,
  ): Promise<PagedResult<Book>> {
    const response = await this.client.query<PagedResult<Book>>(
      API_ENDPOINTS.catalog.books.root,
      query ?? {},
      signal ? { signal } : undefined,
    );

    return response.data;
  }

  public async get(id: string, signal?: AbortSignal): Promise<Book> {
    const response = await this.client.get<Book>(
      API_ENDPOINTS.catalog.books.byId(id),
      signal ? { signal } : undefined,
    );
    return response.data;
  }

  public async getConditional(
    id: string,
    options?: { ifNoneMatch?: string; signal?: AbortSignal },
  ): Promise<ConditionalResponse<Book>> {
    return this.client.getConditional<Book>(
      API_ENDPOINTS.catalog.books.byId(id),
      options,
    );
  }

  private buildFormData(
    request: CreateBookRequest | UpdateBookRequest,
  ): FormData {
    const formData = new FormData();

    if ("id" in request) {
      formData.append("id", request.id);
    }

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

    if ("isRemoveImage" in request && request.isRemoveImage) {
      formData.append("isRemoveImage", "true");
    }

    return formData;
  }

  public async create(request: CreateBookRequest): Promise<Book> {
    const headers = new AxiosHeaders().setContentType("multipart/form-data");

    const response = await this.client.post<Book>(
      API_ENDPOINTS.catalog.books.root,
      this.buildFormData(request),
      {
        headers,
      },
    );
    return response.data;
  }

  public async update(
    request: UpdateBookRequest,
    options?: { ifMatch?: string },
  ): Promise<void> {
    const headers = new AxiosHeaders().setContentType("multipart/form-data");

    if (options?.ifMatch) {
      headers.set(HttpHeaders.IfMatch, options.ifMatch);
    }

    await this.client.put<void>(
      API_ENDPOINTS.catalog.books.root,
      this.buildFormData(request),
      {
        headers,
      },
    );
  }

  public async delete(id: string): Promise<void> {
    await this.client.delete<void>(API_ENDPOINTS.catalog.books.byId(id));
  }
}

export default new BooksApiClient();
