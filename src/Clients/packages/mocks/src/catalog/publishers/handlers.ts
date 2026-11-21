import { HttpResponse, http } from "msw";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
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
  mockUrl,
} from "../../helpers/index";
import { publishersStore } from "./data";

export const publishersHandlers = [
  http.get<never, never, Publisher[]>(
    mockUrl(API_ENDPOINTS.catalog.publishers.root),
    () => {
      const publishers = publishersStore.list();
      return HttpResponse.json(publishers, { status: 200 });
    },
  ),

  http.post<never, CreatePublisherRequest>(
    mockUrl(API_ENDPOINTS.catalog.publishers.root),
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
    mockUrl(API_ENDPOINTS.catalog.publishers.root),
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
    mockUrl(API_ENDPOINTS.catalog.publishers.byId()),
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
