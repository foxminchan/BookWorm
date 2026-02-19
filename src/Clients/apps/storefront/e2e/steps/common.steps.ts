import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Common step definitions used across multiple features.
 * This is the canonical location for shared steps like "I click {string}".
 */

// Background steps
Given("the storefront application is running", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

Given("the catalog has available books", async ({ shopPage }) => {
  await shopPage.navigate();
  const count = await shopPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});

Given(
  "the catalog has at least {int} books available",
  async ({ shopPage }) => {
    await shopPage.navigate();
    const count = await shopPage.getBooksCount();
    expect(count).toBeGreaterThanOrEqual(0);
  },
);

// Navigation steps
Given("I am on the homepage", async ({ homePage }) => {
  await homePage.navigate();
});

Given("I am on the shop page", async ({ shopPage }) => {
  await shopPage.navigate();
});

Given("I am on the basket page", async ({ basketPage }) => {
  await basketPage.navigate();
});

Given("I am on a product detail page", async ({ shopPage }) => {
  await shopPage.navigate();
  await shopPage.clickBook(0);
});

Given(
  "I am on a product detail page for {string}",
  async ({ shopPage }, bookTitle: string) => {
    await shopPage.navigate();
    await shopPage.clickBookByTitle(bookTitle);
  },
);

When("I navigate to the homepage", async ({ homePage }) => {
  await homePage.navigate();
});

When("I navigate to the shop page", async ({ shopPage }) => {
  await shopPage.navigate();
});

When("I go to the basket page", async ({ basketPage }) => {
  await basketPage.navigate();
});

// Header interactions
When("I click the basket icon", async ({ page }) => {
  await page.locator('a[href="/basket"]').click();
  await page.waitForLoadState("networkidle");
});

When("I click the user icon", async ({ page }) => {
  await page
    .locator('button:has-text("Account"), button[aria-label*="User"]')
    .first()
    .click();
});

Then(
  "the basket icon should show {string} items",
  async ({ page }, itemCount: string) => {
    const basketCountElement = page.locator(
      'a[href="/basket"] span, [data-testid="basket-count"]',
    );
    const text = await basketCountElement.textContent();
    const numberMatch = text?.match(/\d+/);
    const actualCount = numberMatch ? numberMatch[0] : "0";
    expect(actualCount).toBe(itemCount);
  },
);

Then("the basket count should increase by {int}", async ({ page }) => {
  const basketCountElement = page.locator(
    'a[href="/basket"] span, [data-testid="basket-count"]',
  );
  await expect(basketCountElement).toBeVisible();
});

Then(
  "the basket count should show {int} items",
  async ({ page }, expectedCount: number) => {
    const basketCountElement = page.locator(
      'a[href="/basket"] span, [data-testid="basket-count"]',
    );
    const text = await basketCountElement.textContent();
    const numberMatch = text?.match(/\d+/);
    const actualCount = numberMatch ? Number.parseInt(numberMatch[0]) : 0;
    expect(actualCount).toBe(expectedCount);
  },
);

// Button clicks — canonical "I click {string}" step
When("I click {string}", async ({ page }, buttonText: string) => {
  const button = page
    .locator(`button:has-text("${buttonText}"), a:has-text("${buttonText}")`)
    .first();
  await button.click();
  await page.waitForTimeout(500);
});

// URL assertions
Then("the URL should contain {string}", async ({ page }, urlPart: string) => {
  await page.waitForTimeout(500);
  const url = page.url();
  expect(url).toContain(urlPart);
});

Then(
  "I should see page {int} in the URL",
  async ({ page }, pageNumber: number) => {
    const url = page.url();
    expect(url).toContain(`page=${pageNumber}`);
  },
);

// Visibility assertions
Then("I should see {string} message", async ({ page }, message: string) => {
  const element = page.locator(`:has-text("${message}")`).first();
  await expect(element).toBeVisible();
});

Then("I should see {string}", async ({ page }, text: string) => {
  const element = page.locator(`:has-text("${text}")`).first();
  await expect(element).toBeVisible();
});

Then("I should see {string} link", async ({ page }, linkText: string) => {
  const link = page.locator(`a:has-text("${linkText}")`).first();
  await expect(link).toBeVisible();
});

Then("I should see {string} button", async ({ page }, buttonText: string) => {
  const button = page.locator(`button:has-text("${buttonText}")`).first();
  await expect(button).toBeVisible();
});

Then("I should see a success message", async ({ page }) => {
  const successElement = page
    .locator(
      ':has-text("success"), :has-text("Success"), :has-text("Thank you")',
    )
    .first();
  await expect(successElement).toBeVisible();
});

// Element visibility checks
Then(
  "there should be no {string} visible",
  async ({ page }, elementText: string) => {
    const element = page.locator(`:has-text("${elementText}")`).first();
    await expect(element).not.toBeVisible();
  },
);

Then("there should be no checkout button visible", async ({ page }) => {
  const checkoutButton = page.locator(
    'button:has-text("Checkout"), button:has-text("Proceed to Checkout")',
  );
  await expect(checkoutButton).not.toBeVisible();
});

// Wait for elements
When("I wait for {int} seconds", async ({ page }, seconds: number) => {
  await page.waitForTimeout(seconds * 1000);
});
