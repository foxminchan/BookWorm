import { Page, test as base, expect } from "@playwright/test";
import { createBdd } from "playwright-bdd";

import { testData, urls } from "../fixtures/test-data";
import {
  BooksPage,
  CustomersPage,
  DashboardPage,
  LoginPage,
  OrdersPage,
  ReviewsPage,
} from "../pages";

/**
 * Custom fixture types for BDD steps
 */
type BddFixtures = {
  loginPage: LoginPage;
  dashboardPage: DashboardPage;
  booksPage: BooksPage;
  ordersPage: OrdersPage;
  customersPage: CustomersPage;
  reviewsPage: ReviewsPage;
  testData: typeof testData;
  urls: typeof urls;
  scenarioData: Record<string, unknown>;
};

/**
 * Extended test instance with page object fixtures
 * Each fixture lazily instantiates the page object from the current `page`
 */
export const test = base.extend<BddFixtures>({
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page));
  },
  dashboardPage: async ({ page }, use) => {
    await use(new DashboardPage(page));
  },
  booksPage: async ({ page }, use) => {
    await use(new BooksPage(page));
  },
  ordersPage: async ({ page }, use) => {
    await use(new OrdersPage(page));
  },
  customersPage: async ({ page }, use) => {
    await use(new CustomersPage(page));
  },
  reviewsPage: async ({ page }, use) => {
    await use(new ReviewsPage(page));
  },
  testData: async ({}, use) => {
    await use(testData);
  },
  urls: async ({}, use) => {
    await use(urls);
  },
  scenarioData: async ({}, use) => {
    await use({});
  },
});

export const { Given, When, Then } = createBdd(test);

export async function expectTableColumns(page: Page, columns: string[]) {
  await Promise.all(
    columns.map((column) =>
      expect(page.locator("table thead th", { hasText: column })).toBeVisible(),
    ),
  );
}
