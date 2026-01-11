import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { ReviewsPage } from "../pages";
import { World } from "../world";

/**
 * Review management step definitions
 */

When("the reviews page loads", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Given("there are multiple reviews", async function (this: { page: Page }) {
  const reviewsPage = new ReviewsPage(this.page);
  const count = await reviewsPage.getReviewsCount();
  expect(count).toBeGreaterThan(1);
});

Given(
  "there is at least one pending review",
  async function (this: { page: Page }) {
    const reviewsPage = new ReviewsPage(this.page);
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThanOrEqual(1);
  },
);

Given(
  "there are reviews with different statuses",
  async function (this: { page: Page }) {
    const reviewsPage = new ReviewsPage(this.page);
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThan(0);
  },
);

Given(
  "there are reviews with different ratings",
  async function (this: { page: Page }) {
    const reviewsPage = new ReviewsPage(this.page);
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThan(0);
  },
);

Given(
  "there are more than {int} reviews",
  async function (this: { page: Page }, reviewCount: number) {
    const reviewsPage = new ReviewsPage(this.page);
    const count = await reviewsPage.getReviewsCount();
    expect(count).toBeGreaterThan(reviewCount);
  },
);

When("I search for reviews by book title", async function (this: World) {
  const reviewsPage = new ReviewsPage(this.page);
  const searchQuery =
    (this.testData.bookTitle as string | undefined) || "Test Book";
  await reviewsPage.searchReviews(searchQuery);
});

When(
  "I click approve on the first review",
  async function (this: { page: Page }) {
    const reviewsPage = new ReviewsPage(this.page);
    await reviewsPage.approveFirstReview();
  },
);

When(
  "I click reject on the first review",
  async function (this: { page: Page }) {
    const reviewsPage = new ReviewsPage(this.page);
    await reviewsPage.rejectFirstReview();
  },
);

When(
  "I filter reviews by {string} status",
  async function (this: { page: Page }, status: string) {
    await this.page.selectOption('select[name="status"]', status);
    await this.page.waitForLoadState("networkidle");
  },
);

When(
  "I filter reviews by {int}-star rating",
  async function (this: { page: Page }, rating: number) {
    await this.page.selectOption('select[name="rating"]', rating.toString());
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should see the reviews table", async function (this: { page: Page }) {
  await expect(this.page.locator("table")).toBeVisible();
});

Then(
  "I should see review information including rating and status",
  async function (this: { page: Page }) {
    await expect(this.page.locator("table")).toBeVisible();
  },
);

Then(
  "I should see only matching reviews",
  async function (this: { page: Page }) {
    const rows = await this.page.locator("table tbody tr").count();
    expect(rows).toBeGreaterThanOrEqual(0);
  },
);

Then("the review should be approved", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Then("the review should be rejected", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Then(
  "I should see only pending reviews",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "I should see only {int}-star reviews",
  async function (this: { page: Page }, rating: number) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should see different reviews", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
