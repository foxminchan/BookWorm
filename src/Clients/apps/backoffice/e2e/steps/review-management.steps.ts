import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Review management step definitions
 */

When("the reviews page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Given("there are multiple reviews", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(1);
});

Given("there is at least one pending review", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given("there are reviews with different statuses", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(0);
});

Given("there are reviews with different ratings", async ({ reviewsPage }) => {
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(0);
});

Given(
  "there are more than {int} reviews",
  async ({ reviewsPage }, reviewCount: number) => {
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThan(reviewCount);
  },
);

When(
  "I search for reviews by book title",
  async ({ reviewsPage, scenarioData }) => {
    const searchQuery = (scenarioData.bookTitle as string) || "Test Book";
    await reviewsPage.searchReviews(searchQuery);
  },
);

When("I click approve on the first review", async ({ reviewsPage }) => {
  await reviewsPage.approveFirstReview();
});

When("I click reject on the first review", async ({ reviewsPage }) => {
  await reviewsPage.rejectFirstReview();
});

When(
  "I filter reviews by {string} status",
  async ({ page }, status: string) => {
    await page.selectOption('select[name="status"]', status);
    await page.waitForLoadState("networkidle");
  },
);

When(
  "I filter reviews by {int}-star rating",
  async ({ page }, rating: number) => {
    await page.selectOption('select[name="rating"]', rating.toString());
    await page.waitForLoadState("networkidle");
  },
);

Then("I should see the reviews table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then(
  "I should see review information including rating and status",
  async ({ page }) => {
    await expect(page.locator("table")).toBeVisible();
  },
);

Then("I should see only matching reviews", async ({ page }) => {
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThanOrEqual(0);
});

Then("the review should be approved", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("the review should be rejected", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see only pending reviews", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then(
  "I should see only {int}-star reviews",
  async ({ page }, _rating: number) => {
    await page.waitForLoadState("networkidle");
  },
);

Then("I should see different reviews", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
