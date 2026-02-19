import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Common step definitions used across multiple features
 */

// Background steps
Given("the backoffice application is running", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

Given("I am logged in as an admin", async ({ page }) => {
  // Mock authentication or perform actual login
  // For now, we'll assume the session is handled via environment or cookies
});

Given("I am not logged in", async ({ page }) => {
  await page.context().clearCookies();
});

// Navigation steps
Given("I am on the homepage", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

Given("I am on the dashboard", async ({ dashboardPage }) => {
  await dashboardPage.navigate();
});

Given("I am on the books page", async ({ booksPage }) => {
  await booksPage.navigate();
});

Given("I am on the orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Given("I am on the customers page", async ({ customersPage }) => {
  await customersPage.navigate();
});

Given("I am on the reviews page", async ({ reviewsPage }) => {
  await reviewsPage.navigate();
});

When("I navigate to the dashboard", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

When("I navigate to {string}", async ({ page }, path: string) => {
  await page.goto(path);
  await page.waitForLoadState("networkidle");
});

When("I navigate to page {int}", async ({ page }, pageNumber: number) => {
  const paginationButton = page.locator(`button:has-text("${pageNumber}")`);
  await paginationButton.click();
  await page.waitForLoadState("networkidle");
});

// New step to handle "I navigate to the books/orders page" used in auth feature
When("I navigate to the books page", async ({ booksPage }) => {
  await booksPage.navigate();
});

When("I navigate to the orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

// Button click steps
When("I click {string}", async ({ page }, buttonText: string) => {
  await page.getByRole("button", { name: buttonText }).click();
});

When(
  "I click on {string} in the navigation",
  async ({ page }, linkText: string) => {
    await page.getByRole("link", { name: linkText }).click();
    await page.waitForLoadState("networkidle");
  },
);

When(
  "I click on {string} in the breadcrumb",
  async ({ page }, linkText: string) => {
    await page
      .locator(`nav[aria-label="breadcrumb"] a:has-text("${linkText}")`)
      .click();
    await page.waitForLoadState("networkidle");
  },
);

// Assertion steps
Then("I should see {string}", async ({ page }, text: string) => {
  await expect(page.locator(`text=${text}`).first()).toBeVisible();
});

Then(
  "I should see the {string} button",
  async ({ page }, buttonText: string) => {
    await expect(page.getByRole("button", { name: buttonText })).toBeVisible();
  },
);

Then("I should see the navigation menu", async ({ page }) => {
  await expect(page.locator("nav")).toBeVisible();
});

Then("I should be on the dashboard", async ({ page }) => {
  await expect(page).toHaveURL("/");
});

Then("I should be on the books page", async ({ page }) => {
  await expect(page).toHaveURL(/\/catalog\/books/);
});

Then("I should be on the orders page", async ({ page }) => {
  await expect(page).toHaveURL(/\/ordering\/orders/);
});

Then("I should be on the customers page", async ({ page }) => {
  await expect(page).toHaveURL(/\/ordering\/customers/);
});

Then("I should be on the reviews page", async ({ page }) => {
  await expect(page).toHaveURL(/\/rating\/reviews/);
});

Then("I should remain authenticated", async ({ page }) => {
  await expect(page.locator("nav")).toBeVisible();
});

Then("I should see a success message", async ({ page }) => {
  await expect(
    page.locator('[role="alert"]:has-text("success")').first(),
  ).toBeVisible();
});

Then("the pagination should update", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

// Data existence checks
Given("the catalog has multiple books", async ({ booksPage }) => {
  await booksPage.navigate();
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});

Given("the catalog has at least one book", async ({ booksPage }) => {
  await booksPage.navigate();
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given(
  "the catalog has more than {int} books",
  async ({ booksPage }, bookCount: number) => {
    await booksPage.navigate();
    const count = await booksPage.getBooksCount();
    expect(count).toBeGreaterThan(bookCount);
  },
);
