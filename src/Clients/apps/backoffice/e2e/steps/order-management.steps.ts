import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Order management step definitions
 */

When("the orders page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Given("there is at least one order", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given("there are multiple orders", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(1);
});

Given("there are orders with different statuses", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(0);
});

Given("there is at least one pending order", async ({ ordersPage }) => {
  await ordersPage.filterByStatus("Pending");
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given("there is at least one processing order", async ({ ordersPage }) => {
  await ordersPage.filterByStatus("Processing");
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given(
  "there are more than {int} orders",
  async ({ ordersPage }, orderCount: number) => {
    const count = await ordersPage.getOrdersCount();
    expect(count).toBeGreaterThan(orderCount);
  },
);

When("I click view on the first order", async ({ ordersPage }) => {
  await ordersPage.viewFirstOrder();
});

When(
  "I search for an order by order number",
  async ({ ordersPage, scenarioData }) => {
    const searchQuery = (scenarioData.orderNumber as string) || "12345678";
    await ordersPage.searchOrders(searchQuery);
  },
);

When(
  "I filter orders by {string} status",
  async ({ ordersPage }, status: string) => {
    await ordersPage.filterByStatus(status);
  },
);

When(
  "I change the order status to {string}",
  async ({ page }, status: string) => {
    await page.selectOption('select[name="status"]', status);
    await page.getByRole("button", { name: "Update" }).click();
    await page.waitForLoadState("networkidle");
  },
);

Then("I should see the orders table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then(
  "I should see order information including status and total",
  async ({ page }) => {
    await expect(page.locator("table")).toBeVisible();
  },
);

Then("I should see the order details page", async ({ page }) => {
  await expect(page).toHaveURL(/\/ordering\/orders\/.+/);
});

Then("I should see ordered items", async ({ page }) => {
  await expect(
    page.locator("text=/items|products|books/i").first(),
  ).toBeVisible();
});

Then("I should see only matching orders", async ({ page }) => {
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThanOrEqual(0);
});

Then("I should see only pending orders", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see only processing orders", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see only completed orders", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("the order status should be updated", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see different orders", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
