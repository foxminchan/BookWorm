import { test as base, createBdd } from "playwright-bdd";

import * as testData from "../fixtures/test-data";
import {
  AboutPage,
  AccountPage,
  BasketPage,
  CheckoutConfirmationPage,
  HomePage,
  LoginPage,
  OrderDetailPage,
  OrdersPage,
  ProductDetailPage,
  RegisterPage,
  ReturnsPage,
  ShippingPage,
  ShopPage,
} from "../pages";

/**
 * Custom fixture types for BDD steps
 */
type BddFixtures = {
  homePage: HomePage;
  shopPage: ShopPage;
  productDetailPage: ProductDetailPage;
  basketPage: BasketPage;
  checkoutConfirmationPage: CheckoutConfirmationPage;
  ordersPage: OrdersPage;
  orderDetailPage: OrderDetailPage;
  loginPage: LoginPage;
  registerPage: RegisterPage;
  accountPage: AccountPage;
  aboutPage: AboutPage;
  shippingPage: ShippingPage;
  returnsPage: ReturnsPage;
  testData: typeof testData;
  scenarioData: Record<string, unknown>;
};

/**
 * Extended test instance with page object fixtures.
 * Each fixture lazily instantiates the page object from the current `page`.
 */
export const test = base.extend<BddFixtures>({
  homePage: async ({ page }, use) => {
    await use(new HomePage(page));
  },
  shopPage: async ({ page }, use) => {
    await use(new ShopPage(page));
  },
  productDetailPage: async ({ page }, use) => {
    await use(new ProductDetailPage(page));
  },
  basketPage: async ({ page }, use) => {
    await use(new BasketPage(page));
  },
  checkoutConfirmationPage: async ({ page }, use) => {
    await use(new CheckoutConfirmationPage(page));
  },
  ordersPage: async ({ page }, use) => {
    await use(new OrdersPage(page));
  },
  orderDetailPage: async ({ page }, use) => {
    await use(new OrderDetailPage(page));
  },
  loginPage: async ({ page }, use) => {
    await use(new LoginPage(page));
  },
  registerPage: async ({ page }, use) => {
    await use(new RegisterPage(page));
  },
  accountPage: async ({ page }, use) => {
    await use(new AccountPage(page));
  },
  aboutPage: async ({ page }, use) => {
    await use(new AboutPage(page));
  },
  shippingPage: async ({ page }, use) => {
    await use(new ShippingPage(page));
  },
  returnsPage: async ({ page }, use) => {
    await use(new ReturnsPage(page));
  },
  testData: async ({}, use) => {
    await use(testData);
  },
  scenarioData: async ({}, use) => {
    await use({});
  },
});

export const { Given, When, Then } = createBdd(test);
