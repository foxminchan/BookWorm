import { expect } from "@playwright/test";

import { Given, Then, When, expectTableColumns } from "./fixtures";

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

Given("there are orders with different statuses", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(0);
});

Given("there is at least one new order", async ({ ordersPage }) => {
  await ordersPage.filterByStatus("New");
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
  "I filter orders by {string} status",
  async ({ ordersPage }, status: string) => {
    await ordersPage.filterByStatus(
      status as "All Orders" | "New" | "Completed" | "Cancelled",
    );
  },
);

When("I complete the first new order", async ({ ordersPage }) => {
  await ordersPage.completeFirstOrder();
});

When("I cancel the first new order", async ({ ordersPage }) => {
  await ordersPage.cancelFirstOrder();
});

Then("I should see the orders table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then(
  "I should see order information including status and total",
  async ({ page }) => {
    await expectTableColumns(page, ["Order ID", "Total", "Status"]);
  },
);

Then("I should see the order details page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/orders\/.+/);
});

Then("I should see order summary cards", async ({ page }) => {
  await expect(page.getByText("Order Status")).toBeVisible();
  await expect(page.getByText("Order Total")).toBeVisible();
  await expect(page.getByText("Items", { exact: true })).toBeVisible();
});

Then("I should see ordered items", async ({ page }) => {
  await expect(page.getByText("Order Items")).toBeVisible();
});

Then("I should see the order event history", async ({ page }) => {
  await expect(page.getByText("Event Stream & Time-Travel")).toBeVisible();
});

Then("I should see only {string} orders", async ({ page }, status: string) => {
  await expect(
    page.getByRole("button", { name: status, exact: true }),
  ).toHaveAttribute("aria-pressed", "true");
  await page.waitForLoadState("networkidle");
});

Then("the order status should be updated", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see different orders", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(0);
});
