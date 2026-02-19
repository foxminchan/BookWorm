import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Navigation step definitions
 */

Then("I should see the dashboard statistics", async ({ dashboardPage }) => {
  const statsCount = await dashboardPage.getStatsCount();
  expect(statsCount).toBeGreaterThan(0);
});

Given("I am using a mobile device", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
});

When("I access the backoffice", async ({ page }) => {
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

Then("I should see a mobile blocker message", async ({ page }) => {
  await expect(
    page.locator("text=/mobile|tablet|desktop/i").first(),
  ).toBeVisible();
});

Then("I should not see the main content", async ({ page }) => {
  const isNavVisible = await page.locator("nav").isVisible();
  expect(isNavVisible).toBeFalsy();
});
