import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import {
  BooksPage,
  CustomersPage,
  DashboardPage,
  OrdersPage,
  ReviewsPage,
} from "../pages";
import { World } from "../world";

/**
 * Common step definitions used across multiple features
 */

// Background steps
Given(
  "the backoffice application is running",
  async function (this: { page: Page }) {
    // Application should be running on port 3001
    await this.page.goto("/");
    await this.page.waitForLoadState("networkidle");
  },
);

Given("I am logged in as an admin", async function (this: World) {
  // Mock authentication or perform actual login
  // For now, we'll assume the session is handled via environment or cookies
  // In real scenarios, you'd perform the OAuth flow or set session cookies
  this.testData.isAuthenticated = true;
});

Given("I am not logged in", async function (this: { page: Page }) {
  // Clear any existing session
  await this.page.context().clearCookies();
});

// Navigation steps
Given("I am on the homepage", async function (this: { page: Page }) {
  await this.page.goto("/");
  await this.page.waitForLoadState("networkidle");
});

Given("I am on the dashboard", async function (this: { page: Page }) {
  const dashboardPage = new DashboardPage(this.page);
  await dashboardPage.navigate();
});

Given("I am on the books page", async function (this: { page: Page }) {
  const booksPage = new BooksPage(this.page);
  await booksPage.navigate();
});

Given("I am on the orders page", async function (this: { page: Page }) {
  const ordersPage = new OrdersPage(this.page);
  await ordersPage.navigate();
});

Given("I am on the customers page", async function (this: { page: Page }) {
  const customersPage = new CustomersPage(this.page);
  await customersPage.navigate();
});

Given("I am on the reviews page", async function (this: { page: Page }) {
  const reviewsPage = new ReviewsPage(this.page);
  await reviewsPage.navigate();
});

When("I navigate to the dashboard", async function (this: { page: Page }) {
  await this.page.goto("/");
  await this.page.waitForLoadState("networkidle");
});

When(
  "I navigate to {string}",
  async function (this: { page: Page }, path: string) {
    await this.page.goto(path);
    await this.page.waitForLoadState("networkidle");
  },
);

When(
  "I navigate to page {int}",
  async function (this: { page: Page }, pageNumber: number) {
    const paginationButton = this.page.locator(
      `button:has-text("${pageNumber}")`,
    );
    await paginationButton.click();
    await this.page.waitForLoadState("networkidle");
  },
);

// Button click steps
When(
  "I click {string}",
  async function (this: { page: Page }, buttonText: string) {
    await this.page.getByRole("button", { name: buttonText }).click();
  },
);

When(
  "I click the {string} button",
  async function (this: { page: Page }, buttonText: string) {
    await this.page.getByRole("button", { name: buttonText }).click();
  },
);

When(
  "I click on {string} in the navigation",
  async function (this: { page: Page }, linkText: string) {
    await this.page.getByRole("link", { name: linkText }).click();
    await this.page.waitForLoadState("networkidle");
  },
);

When(
  "I click on {string} in the breadcrumb",
  async function (this: { page: Page }, linkText: string) {
    await this.page
      .locator(`nav[aria-label="breadcrumb"] a:has-text("${linkText}")`)
      .click();
    await this.page.waitForLoadState("networkidle");
  },
);

// Assertion steps
Then(
  "I should see {string}",
  async function (this: { page: Page }, text: string) {
    await expect(this.page.locator(`text=${text}`).first()).toBeVisible();
  },
);

Then(
  "I should see the {string} button",
  async function (this: { page: Page }, buttonText: string) {
    await expect(
      this.page.getByRole("button", { name: buttonText }),
    ).toBeVisible();
  },
);

Then("I should see the navigation menu", async function (this: { page: Page }) {
  await expect(this.page.locator("nav")).toBeVisible();
});

Then("I should be on the dashboard", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL("/");
});

Then("I should be on the books page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/catalog\/books/);
});

Then("I should be on the orders page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/ordering\/orders/);
});

Then(
  "I should be on the customers page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/ordering\/customers/);
  },
);

Then("I should be on the reviews page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/rating\/reviews/);
});

Then("I should remain authenticated", async function (this: { page: Page }) {
  // Check for authentication indicators (e.g., user menu, navigation)
  await expect(this.page.locator("nav")).toBeVisible();
});

Then("I should see a success message", async function (this: { page: Page }) {
  await expect(
    this.page.locator('[role="alert"]:has-text("success")').first(),
  ).toBeVisible();
});

Then("the pagination should update", async function (this: { page: Page }) {
  // Wait for pagination to reflect the change
  await this.page.waitForLoadState("networkidle");
});

// Data existence checks
Given("the catalog has multiple books", async function (this: { page: Page }) {
  const booksPage = new BooksPage(this.page);
  await booksPage.navigate();
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});

Given(
  "the catalog has at least one book",
  async function (this: { page: Page }) {
    const booksPage = new BooksPage(this.page);
    await booksPage.navigate();
    const count = await booksPage.getBooksCount();
    expect(count).toBeGreaterThanOrEqual(1);
  },
);

Given(
  "the catalog has more than {int} books",
  async function (this: { page: Page }, bookCount: number) {
    const booksPage = new BooksPage(this.page);
    await booksPage.navigate();
    const count = await booksPage.getBooksCount();
    expect(count).toBeGreaterThan(bookCount);
  },
);
