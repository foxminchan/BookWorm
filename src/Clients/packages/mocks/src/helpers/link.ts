/**
 * Build a Link header value for pagination
 * @param requestUrl - The original request URL
 * @param page - The page number for this link
 * @param pageSize - The page size
 * @param rel - The relationship (first, self, previous, next, last)
 * @returns A formatted link header value
 */
export function buildPaginationLink(
  requestUrl: string,
  page: number,
  pageSize: number,
  rel: string,
): string {
  const url = new URL(requestUrl);
  url.searchParams.set("pageIndex", page.toString());
  url.searchParams.set("pageSize", pageSize.toString());
  return `<${url.pathname}${url.search}>;rel=${rel}`;
}

/**
 * Build all pagination links for a paged result
 * @param requestUrl - The original request URL
 * @param pageIndex - Current page index
 * @param pageSize - Page size
 * @param totalPages - Total number of pages
 * @param hasPreviousPage - Whether there is a previous page
 * @param hasNextPage - Whether there is a next page
 * @returns Array of link header values
 */
export function buildPaginationLinks(
  requestUrl: string,
  pageIndex: number,
  pageSize: number,
  totalPages: number,
  hasPreviousPage: boolean,
  hasNextPage: boolean,
): string[] {
  const links = [
    buildPaginationLink(requestUrl, 1, pageSize, "first"),
    buildPaginationLink(requestUrl, pageIndex, pageSize, "self"),
  ];

  if (hasPreviousPage && pageIndex > 1) {
    links.push(
      buildPaginationLink(requestUrl, pageIndex - 1, pageSize, "previous"),
    );
  }

  if (hasNextPage && pageIndex < totalPages) {
    links.push(
      buildPaginationLink(requestUrl, pageIndex + 1, pageSize, "next"),
    );
  }

  links.push(buildPaginationLink(requestUrl, totalPages, pageSize, "last"));

  return links;
}
