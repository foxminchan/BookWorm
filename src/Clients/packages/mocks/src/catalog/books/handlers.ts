import { HttpResponse, http } from "msw";

import type {
  Book,
  CreateBookRequest,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import { PagedResult } from "@workspace/types/shared";
import { buildPaginationLinks } from "@workspace/utils/link";
import { generateTraceId } from "@workspace/utils/trace";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createBookSchema,
  updateBookSchema,
} from "@workspace/validations/catalog/books";

import { CATALOG_API_BASE_URL } from "../../catalog/constants";
import { booksStore } from "./data";

export const booksHandlers = [
  http.get<never, never, PagedResult<Book>>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    ({ request }) => {
      const url = new URL(request.url);
      const pageIndex = Number.parseInt(
        url.searchParams.get("pageIndex") || "0",
      );
      const pageSize = Number.parseInt(
        url.searchParams.get("pageSize") || "10",
      );
      const searchTerm = url.searchParams.get("search") || undefined;

      // Convert 0-indexed to 1-indexed for the store
      const result = booksStore.list({
        pageIndex: pageIndex + 1,
        pageSize,
        searchTerm,
      });

      const links = buildPaginationLinks(
        request.url,
        result.pageIndex,
        result.pageSize,
        result.totalPages,
        result.hasPreviousPage,
        result.hasNextPage,
      );

      const headers = new Headers();
      headers.set("Link", links.join(","));
      headers.set("Pagination-Count", result.totalCount.toString());

      const response: PagedResult<Book> = {
        items: result.items,
        totalCount: result.totalCount,
        link: links.join(","),
      };

      return HttpResponse.json(response, { status: 200, headers });
    },
  ),

  http.get<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/books/:id`,
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Id: ["'Id' must not be empty."],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const book = booksStore.get(id);

      if (!book) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return HttpResponse.json(book, { status: 200 });
    },
  ),

  http.post<never, CreateBookRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    async ({ request }) => {
      const contentType = request.headers.get("Content-Type");
      if (!contentType?.includes("multipart/form-data")) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              ContentType: [
                "Content-Type must be multipart/form-data for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const formData = await request.formData();
      const body: CreateBookRequest = {
        name: formData.get("name") as string,
        description: formData.get("description") as string,
        price: Number.parseFloat(formData.get("price") as string),
        priceSale: formData.get("priceSale")
          ? Number.parseFloat(formData.get("priceSale") as string)
          : null,
        categoryId: formData.get("categoryId") as string,
        publisherId: formData.get("publisherId") as string,
        authorIds: formData.getAll("authorIds") as string[],
        image: formData.get("image") as File | undefined,
      };

      const result = createBookSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newBookId = booksStore.create(result.data);
      const createdBook = booksStore.get(newBookId);

      const headers = new Headers();
      headers.set("Location", `/api/v1/books/${newBookId}`);

      return HttpResponse.json(createdBook, { status: 201, headers });
    },
  ),

  http.put<never, UpdateBookRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/books`,
    async ({ request }) => {
      const contentType = request.headers.get("Content-Type");
      if (!contentType?.includes("multipart/form-data")) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              ContentType: [
                "Content-Type must be multipart/form-data for this endpoint.",
              ],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const formData = await request.formData();
      const body: UpdateBookRequest = {
        id: formData.get("id") as string,
        name: formData.get("name") as string,
        description: formData.get("description") as string,
        price: Number.parseFloat(formData.get("price") as string),
        priceSale: formData.get("priceSale")
          ? Number.parseFloat(formData.get("priceSale") as string)
          : null,
        categoryId: formData.get("categoryId") as string,
        publisherId: formData.get("publisherId") as string,
        authorIds: formData.getAll("authorIds") as string[],
        image: formData.get("image") as File | undefined,
        isRemoveImage: formData.get("isRemoveImage") === "true",
      };

      const result = updateBookSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = booksStore.update(result.data);

      if (!updated) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/books/:id`,
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return HttpResponse.json(
          {
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            title: "One or more validation errors occurred.",
            status: 400,
            errors: {
              Id: ["'Id' must not be empty."],
            },
            traceId: generateTraceId(),
          },
          { status: 400 },
        );
      }

      const deleted = booksStore.delete(id);

      if (!deleted) {
        return new HttpResponse("Book not found", { status: 404 });
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
