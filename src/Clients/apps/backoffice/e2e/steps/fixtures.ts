import { test as base, createBdd } from "playwright-bdd";

import { credentials, testData, urls } from "../fixtures/test-data";
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
  credentials: typeof credentials;
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
  credentials: async ({}, use) => {
    await use(credentials);
  },
  urls: async ({}, use) => {
    await use(urls);
  },
  scenarioData: async ({}, use) => {
    await use({});
  },
});

export const { Given, When, Then } = createBdd(test);
