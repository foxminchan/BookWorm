import { HttpResponse, http, passthrough } from "msw";
import * as v from "valibot";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import { HttpHeaders } from "@workspace/api-client/headers";
import type {
  Book,
  CreateBookRequest,
  ListBooksQuery,
  UpdateBookRequest,
} from "@workspace/types/catalog/books";
import { PagedResult } from "@workspace/types/shared";
import { buildPaginationLinks } from "@workspace/utils/link";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createBookSchema,
  updateBookSchema,
} from "@workspace/validations/catalog/books";

import {
  createCorsHeaders,
  createNotFoundResponse,
  createPreconditionFailedResponse,
  createPreflightResponse,
  createValidationErrorResponse,
  mockUrl,
  withCorsHeaders,
} from "../../helpers/index";
import { booksStore } from "./data";

function matchesETagHeader(
  headerValue: string | null,
  currentETag: string,
): boolean {
  if (!headerValue) {
    return false;
  }

  return headerValue
    .split(",")
    .map((tag) => tag.trim().replace(/^W\//, ""))
    .some((tag) => tag === "*" || tag === currentETag);
}

async function parseBookFormData(request: Request) {
  const contentType = request.headers.get(HttpHeaders.ContentType);
  if (!contentType?.includes("multipart/form-data")) {
    return {
      error: createValidationErrorResponse({
        ContentType: [
          "Content-Type must be multipart/form-data for this endpoint.",
        ],
      }),
    };
  }

  const formData = await request.formData();
  return {
    formData,
    baseBody: {
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
    },
  };
}

export const booksHandlers = [
  http.options(mockUrl(API_ENDPOINTS.catalog.books.root), ({ request }) =>
    createPreflightResponse(request),
  ),

  http.options(mockUrl(API_ENDPOINTS.catalog.books.byId()), ({ request }) =>
    createPreflightResponse(request),
  ),

  http.get<{ id: string }>(
    mockUrl(API_ENDPOINTS.catalog.books.byId()),
    ({ params, request }) => {
      const { id } = params;

      if (!id) {
        return createValidationErrorResponse({
          Id: ["'Id' must not be empty."],
        });
      }

      const book = booksStore.get(id);

      if (!book) {
        return createNotFoundResponse("Book");
      }

      const etag = `"${book.version ?? 1}"`;

      if (
        matchesETagHeader(request.headers.get(HttpHeaders.IfNoneMatch), etag)
      ) {
        return new HttpResponse(null, {
          status: 304,
          headers: createCorsHeaders(request, { [HttpHeaders.ETag]: etag }),
        });
      }

      return HttpResponse.json(book, {
        status: 200,
        headers: createCorsHeaders(request, { [HttpHeaders.ETag]: etag }),
      });
    },
  ),

  http.post<never, CreateBookRequest>(
    mockUrl(API_ENDPOINTS.catalog.books.root),
    async ({ request }) => {
      const parsed = await parseBookFormData(request);
      if ("error" in parsed) {
        return parsed.error;
      }

      const result = v.safeParse(createBookSchema, parsed.baseBody);

      if (!result.success) {
        return HttpResponse.json(
          formatValidationErrors({ issues: result.issues }),
          {
            status: 400,
          },
        );
      }

      const newBookId = booksStore.create(result.output);
      const createdBook = booksStore.get(newBookId);

      const headers = new Headers();
      headers.set(
        HttpHeaders.Location,
        mockUrl(API_ENDPOINTS.catalog.books.byId(newBookId)),
      );

      return HttpResponse.json(createdBook, { status: 201, headers });
    },
  ),

  http.put<never, UpdateBookRequest>(
    mockUrl(API_ENDPOINTS.catalog.books.root),
    async ({ request }) => {
      const parsed = await parseBookFormData(request);
      if (parsed.error) {
        return withCorsHeaders(request, parsed.error);
      }

      const body: UpdateBookRequest = {
        ...parsed.baseBody,
        id: parsed.formData.get("id") as string,
        isRemoveImage: parsed.formData.get("isRemoveImage") === "true",
      };

      const result = v.safeParse(updateBookSchema, body);

      if (!result.success) {
        return HttpResponse.json(
          formatValidationErrors({ issues: result.issues }),
          {
            status: 400,
            headers: createCorsHeaders(request),
          },
        );
      }

      const existingBook = booksStore.get(result.output.id);

      if (!existingBook) {
        return withCorsHeaders(request, createNotFoundResponse("Book"));
      }

      const currentETag = `"${existingBook.version ?? 1}"`;
      const ifMatch = request.headers.get(HttpHeaders.IfMatch)?.trim();
      if (ifMatch && !matchesETagHeader(ifMatch, currentETag)) {
        return withCorsHeaders(request, createPreconditionFailedResponse());
      }

      booksStore.update(result.output);

      return new HttpResponse(null, {
        status: 204,
        headers: createCorsHeaders(request),
      });
    },
  ),

  http.all<never, ListBooksQuery, PagedResult<Book>>(
    mockUrl(API_ENDPOINTS.catalog.books.root),
    async ({ request }) => {
      if (request.method !== "QUERY") {
        return passthrough();
      }

      const body = (await request.json().catch(() => ({}))) as ListBooksQuery;
      const pageIndex = body.pageIndex ?? 0;
      const pageSize = body.pageSize ?? 10;
      const searchTerm = body.search || undefined;

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

      const headers = new Headers(createCorsHeaders(request));
      headers.set(HttpHeaders.Link, links.join(","));
      headers.set(HttpHeaders.PaginationCount, result.totalCount.toString());

      const response: PagedResult<Book> = {
        items: result.items,
        totalCount: result.totalCount,
        link: links.join(","),
      };

      return HttpResponse.json(response, { status: 200, headers });
    },
  ),

  http.delete<{ id: string }>(
    mockUrl(API_ENDPOINTS.catalog.books.byId()),
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return createValidationErrorResponse({
          Id: ["'Id' must not be empty."],
        });
      }

      const deleted = booksStore.delete(id);

      if (!deleted) {
        return createNotFoundResponse("Book");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
