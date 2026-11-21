import { HttpResponse, http } from "msw";

import { API_ENDPOINTS } from "@workspace/api-client/endpoints";
import { HttpHeaders } from "@workspace/api-client/headers";
import { buildPaginationLinks } from "@workspace/utils/link";
import { formatValidationErrors } from "@workspace/utils/validation";
import {
  createBuyerSchema,
  updateAddressSchema,
} from "@workspace/validations/ordering/buyers";

import { mockUrl } from "../../helpers/index";
import { MOCK_USER_ID, buyersStoreManager } from "./data";

export const buyersHandlers = [
  http.get(mockUrl(API_ENDPOINTS.ordering.buyers.me), () => {
    const buyer = buyersStoreManager.getCurrent();

    if (!buyer) {
      return new HttpResponse(null, { status: 404 });
    }

    return HttpResponse.json(buyer, { status: 200 });
  }),

  http.get(mockUrl(API_ENDPOINTS.ordering.buyers.root), ({ request }) => {
    const url = new URL(request.url);
    const pageIndex = Number.parseInt(url.searchParams.get("pageIndex") || "1");
    const pageSize = Number.parseInt(url.searchParams.get("pageSize") || "10");

    const allBuyers = buyersStoreManager.list();
    const totalCount = allBuyers.length;
    const totalPages = Math.ceil(totalCount / pageSize);
    const startIndex = (pageIndex - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const items = allBuyers.slice(startIndex, endIndex);

    const hasPreviousPage = pageIndex > 1;
    const hasNextPage = pageIndex < totalPages;

    const links = buildPaginationLinks(
      request.url,
      pageIndex,
      pageSize,
      totalPages,
      hasPreviousPage,
      hasNextPage,
    );

    const headers = new Headers();
    headers.set(HttpHeaders.Link, links.join(","));
    headers.set(HttpHeaders.PaginationCount, totalCount.toString());

    return HttpResponse.json(items, { status: 200, headers });
  }),

  http.post(
    mockUrl(API_ENDPOINTS.ordering.buyers.root),
    async ({ request }) => {
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
        HttpHeaders.Location,
        mockUrl(API_ENDPOINTS.ordering.buyers.byId(buyer.id)),
      );

      return HttpResponse.json(buyer.id, { status: 201, headers });
    },
  ),

  http.patch(
    mockUrl(API_ENDPOINTS.ordering.buyers.address),
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

  http.delete(mockUrl(API_ENDPOINTS.ordering.buyers.byId()), ({ params }) => {
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
