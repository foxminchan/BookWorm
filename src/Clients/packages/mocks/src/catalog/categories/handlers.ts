import { HttpResponse, http } from "msw";

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
} from "../../helpers/index";
import { CATALOG_API_BASE_URL } from "../constants";
import { categoriesStore } from "./data";

export const categoriesHandlers = [
  http.get<never, never, Category[]>(
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
    () => {
      const categories = categoriesStore.list();
      return HttpResponse.json(categories, { status: 200 });
    },
  ),

  http.post<never, CreateCategoryRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
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
    `${CATALOG_API_BASE_URL}/api/v1/categories`,
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
    `${CATALOG_API_BASE_URL}/api/v1/categories/:id`,
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
