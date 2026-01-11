import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { DashboardPage } from "../pages";

/**
 * Navigation step definitions
 */

Then(
  "I should see the dashboard statistics",
  async function (this: { page: Page }) {
    const dashboardPage = new DashboardPage(this.page);
    const statsCount = await dashboardPage.getStatsCount();
    expect(statsCount).toBeGreaterThan(0);
  },
);

Given("I am using a mobile device", async function (this: { page: Page }) {
  await this.page.setViewportSize({ width: 375, height: 667 });
});

When("I access the backoffice", async function (this: { page: Page }) {
  await this.page.goto("/");
  await this.page.waitForLoadState("networkidle");
});

Then(
  "I should see a mobile blocker message",
  async function (this: { page: Page }) {
    await expect(
      this.page.locator("text=/mobile|tablet|desktop/i").first(),
    ).toBeVisible();
  },
);

Then(
  "I should not see the main content",
  async function (this: { page: Page }) {
    const isNavVisible = await this.page.locator("nav").isVisible();
    expect(isNavVisible).toBeFalsy();
  },
);
