import { HttpResponse, http } from "msw";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import type {
  Category,
  CreateCategoryRequest,
  UpdateCategoryRequest,
} from "@workspace/types/catalog/categories";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createCategorySchema,
  updateCategorySchema,
} from "@workspace/validations/catalog/categories";

import {
  createNotFoundResponse,
  createValidationErrorResponse,
  mockUrl,
} from "../../helpers/index";
import { categoriesStore } from "./data";

export const categoriesHandlers = [
  http.get<never, never, Category[]>(
    mockUrl(API_ENDPOINTS.catalog.categories.root),
    () => {
      const categories = categoriesStore.list();
      return HttpResponse.json(categories, { status: 200 });
    },
  ),

  http.post<never, CreateCategoryRequest>(
    mockUrl(API_ENDPOINTS.catalog.categories.root),
    async ({ request }) => {
      const body = await request.json();
      const result = createCategorySchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newCategoryId = categoriesStore.create(result.data.name);
      return HttpResponse.json(newCategoryId, { status: 200 });
    },
  ),

  http.put<never, UpdateCategoryRequest>(
    mockUrl(API_ENDPOINTS.catalog.categories.root),
    async ({ request }) => {
      const body = await request.json();
      const result = updateCategorySchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = categoriesStore.update(result.data.id, result.data.name);

      if (!updated) {
        return createNotFoundResponse("Category");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    mockUrl(API_ENDPOINTS.catalog.categories.byId()),
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return createValidationErrorResponse({
          Id: ["'Id' must not be empty."],
        });
      }

      const deleted = categoriesStore.delete(id);

      if (!deleted) {
        return createNotFoundResponse("Category");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
