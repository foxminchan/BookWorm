import { http, HttpResponse } from "msw";
import type { Buyer } from "@workspace/types/ordering/buyers";
import {
  createBuyerSchema,
  updateAddressSchema,
} from "@workspace/validations/ordering/buyers";
import { buyersStoreManager, MOCK_USER_ID } from "./data";
import { formatValidationErrors } from "@workspace/utils/validation";
import { buildPaginationLinks } from "@workspace/utils/link";
import { ORDERING_API_BASE_URL } from "../constants";
import { PagedResult } from "@workspace/types/shared";

export const buyersHandlers = [
  http.get(`${ORDERING_API_BASE_URL}/api/v1/buyers/me`, () => {
    const buyer = buyersStoreManager.getCurrentBuyer();

    if (!buyer) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(buyer, { status: 200 });
  }),

  http.get(`${ORDERING_API_BASE_URL}/api/v1/buyers`, ({ request }) => {
    const url = new URL(request.url);
    const pageIndex = Number.parseInt(url.searchParams.get("pageIndex") || "1");
    const pageSize = Number.parseInt(url.searchParams.get("pageSize") || "10");

    const allBuyers = buyersStoreManager.getAll();
    const totalCount = allBuyers.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageIndex - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const items = allBuyers.slice(startIndex, endIndex);

    const result: PagedResult<Buyer> = {
      items,
      pageIndex,
      pageSize,
      totalCount,
      totalPages,
      hasPreviousPage: pageIndex > 1,
      hasNextPage: pageIndex < totalPages,
    };

    const links = buildPaginationLinks(
      request.url,
      pageIndex,
      pageSize,
      totalPages,
      result.hasPreviousPage,
      result.hasNextPage,
    );

    const headers = new Headers();
    headers.set("Link", links.join(","));
    headers.set("Pagination-Count", totalCount.toString());

    return HttpResponse.json(result, { status: 200, headers });
  }),

  http.post(`${ORDERING_API_BASE_URL}/api/v1/buyers`, async ({ request }) => {
    const body = await request.json();
    const result = createBuyerSchema.safeParse(body);

    if (!result.success) {
      return HttpResponse.json(formatValidationErrors(result.error), {
        status: 400,
      });
    }

    const buyer = buyersStoreManager.create(
      "Anonymous",
      result.data.street,
      result.data.city,
      result.data.province,
    );

    const headers = new Headers();
    headers.set(
      "Location",
      `${ORDERING_API_BASE_URL}/api/v1/buyers/${buyer.id}`,
    );

    return HttpResponse.json(buyer.id, { status: 201, headers });
  }),

  http.patch(
    `${ORDERING_API_BASE_URL}/api/v1/buyers/address`,
    async ({ request }) => {
      const body = await request.json();
      const result = updateAddressSchema.safeParse(body);

      if (!result.success) {
        return HttpResponse.json(formatValidationErrors(result.error), {
          status: 400,
        });
      }

      const buyer = buyersStoreManager.updateAddress(
        MOCK_USER_ID,
        result.data.street,
        result.data.city,
        result.data.province,
      );

      if (!buyer) {
        return new HttpResponse(null, { status: 404 });
      }

      return HttpResponse.json(buyer, { status: 200 });
    },
  ),

  http.delete(`${ORDERING_API_BASE_URL}/api/v1/buyers/:id`, ({ params }) => {
    const { id } = params;

    if (!id || typeof id !== "string") {
      return new HttpResponse(null, { status: 400 });
    }

    const deleted = buyersStoreManager.delete(id);

    if (!deleted) {
      return new HttpResponse(null, { status: 404 });
    }

    return new HttpResponse(null, { status: 204 });
  }),
];
