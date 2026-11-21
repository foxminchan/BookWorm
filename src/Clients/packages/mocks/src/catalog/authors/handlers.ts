import { HttpResponse, http } from "msw";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import type {
  Author,
  CreateAuthorRequest,
  UpdateAuthorRequest,
} from "@workspace/types/catalog/authors";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createAuthorSchema,
  updateAuthorSchema,
} from "@workspace/validations/catalog/authors";

import {
  createNotFoundResponse,
  createValidationErrorResponse,
  mockUrl,
} from "../../helpers/index";
import { authorsStore } from "./data";

export const authorsHandlers = [
  http.get<never, never, Author[]>(
    mockUrl(API_ENDPOINTS.catalog.authors.root),
    () => {
      const authors = authorsStore.list();
      return HttpResponse.json(authors, { status: 200 });
    },
  ),

  http.post<never, CreateAuthorRequest>(
    mockUrl(API_ENDPOINTS.catalog.authors.root),
    async ({ request }) => {
      const body = await request.json();
      const result = createAuthorSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newAuthorId = authorsStore.create(result.data.name);
      return HttpResponse.json(newAuthorId, { status: 200 });
    },
  ),

  http.put<never, UpdateAuthorRequest>(
    mockUrl(API_ENDPOINTS.catalog.authors.root),
    async ({ request }) => {
      const body = await request.json();
      const result = updateAuthorSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = authorsStore.update(result.data.id, result.data.name);

      if (!updated) {
        return createNotFoundResponse("Author");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    mockUrl(API_ENDPOINTS.catalog.authors.byId()),
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return createValidationErrorResponse({
          Id: ["'Id' must not be empty."],
        });
      }

      const deleted = authorsStore.delete(id);

      if (!deleted) {
        return createNotFoundResponse("Author");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
