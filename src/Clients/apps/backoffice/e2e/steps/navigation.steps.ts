import { expect } from "@playwright/test";

import { BASE_PATH } from "../fixtures/test-data";
import { Given, Then, When } from "./fixtures";

/**
 * Navigation step definitions
 */

Then("I should see the dashboard statistics", async ({ dashboardPage }) => {
  const statsCount = await dashboardPage.getStatsCount();
  expect(statsCount).toBe(4);
});

Given("I am using a mobile device", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
});

When("I access the backoffice", async ({ page }) => {
  await page.goto(BASE_PATH);
  await page.waitForLoadState("networkidle");
});

Then("I should see a mobile blocker message", async ({ page }) => {
  await expect(
    page.getByRole("heading", { name: "Desktop Only" }),
  ).toBeVisible();
  await expect(
    page.getByText("The Admin Portal is only available on desktop devices."),
  ).toBeVisible();
});

Then("I should not see the main content", async ({ page }) => {
  const isNavVisible = await page
    .locator('nav[aria-label="Main navigation"]')
    .isVisible();
  expect(isNavVisible).toBeFalsy();
});
