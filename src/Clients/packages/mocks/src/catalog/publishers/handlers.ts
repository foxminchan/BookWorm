import { HttpResponse, http } from "msw";

import type {
  CreatePublisherRequest,
  Publisher,
  UpdatePublisherRequest,
} from "@workspace/types/catalog/publishers";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createPublisherSchema,
  updatePublisherSchema,
} from "@workspace/validations/catalog/publishers";

import {
  createNotFoundResponse,
  createValidationErrorResponse,
} from "../../helpers/index";
import { CATALOG_API_BASE_URL } from "../constants";
import { publishersStore } from "./data";

export const publishersHandlers = [
  http.get<never, never, Publisher[]>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
    () => {
      const publishers = publishersStore.list();
      return HttpResponse.json(publishers, { status: 200 });
    },
  ),

  http.post<never, CreatePublisherRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
    async ({ request }) => {
      const body = await request.json();
      const result = createPublisherSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const newPublisherId = publishersStore.create(result.data.name);
      return HttpResponse.json(newPublisherId, { status: 200 });
    },
  ),

  http.put<never, UpdatePublisherRequest>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers`,
    async ({ request }) => {
      const body = await request.json();
      const result = updatePublisherSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const updated = publishersStore.update(result.data.id, result.data.name);

      if (!updated) {
        return createNotFoundResponse("Publisher");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),

  http.delete<{ id: string }>(
    `${CATALOG_API_BASE_URL}/api/v1/publishers/:id`,
    ({ params }) => {
      const { id } = params;

      if (!id) {
        return createValidationErrorResponse({
          Id: ["'Id' must not be empty."],
        });
      }

      const deleted = publishersStore.delete(id);

      if (!deleted) {
        return createNotFoundResponse("Publisher");
      }

      return new HttpResponse(null, { status: 204 });
    },
  ),
];
