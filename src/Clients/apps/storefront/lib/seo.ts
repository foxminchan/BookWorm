import {
  getCanonicalUrl as createCanonicalUrl,
  generateItemListJsonLd as createItemListJsonLd,
  generateOrganizationJsonLd as createOrganizationJsonLd,
  generateWebsiteJsonLd as createWebsiteJsonLd,
} from "@workspace/seo/schema";
import type { Book } from "@workspace/types/catalog/books";

import { APP_CONFIG } from "@/config/app-config";
import { env } from "@/env.mjs";

export {
  buildQueryString,
  generateBreadcrumbJsonLd,
  generateCollectionPageJsonLd,
  generateFAQJsonLd,
  generateImageObject,
  generateProductJsonLd,
  truncateDescription,
} from "@workspace/seo/schema";

const getBaseUrl = () => env.NEXT_PUBLIC_APP_URL || APP_CONFIG.defaultUrl;

export function generateOrganizationJsonLd() {
  return createOrganizationJsonLd({
    name: APP_CONFIG.name,
    sameAs: [APP_CONFIG.social.twitter, APP_CONFIG.social.facebook],
    supportEmail: APP_CONFIG.email.support,
  });
}

export function generateWebsiteJsonLd() {
  return createWebsiteJsonLd({
    name: APP_CONFIG.name,
    baseUrl: getBaseUrl(),
  });
}

export function generateItemListJsonLd(books: Book[], listName: string) {
  return createItemListJsonLd(books, listName, getBaseUrl());
}

export function getCanonicalUrl(path: string): string {
  return createCanonicalUrl(path, getBaseUrl());
}
