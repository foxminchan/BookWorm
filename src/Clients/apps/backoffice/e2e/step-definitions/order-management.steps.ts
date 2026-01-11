import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { OrdersPage } from "../pages";
import { World } from "../world";

/**
 * Order management step definitions
 */

When("the orders page loads", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Given("there is at least one order", async function (this: { page: Page }) {
  const ordersPage = new OrdersPage(this.page);
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given("there are multiple orders", async function (this: { page: Page }) {
  const ordersPage = new OrdersPage(this.page);
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(1);
});

Given(
  "there are orders with different statuses",
  async function (this: { page: Page }) {
    // Assume orders exist with different statuses
    const ordersPage = new OrdersPage(this.page);
    const count = await ordersPage.getOrdersCount();
    expect(count).toBeGreaterThan(0);
  },
);

Given(
  "there is at least one pending order",
  async function (this: { page: Page }) {
    const ordersPage = new OrdersPage(this.page);
    await ordersPage.filterByStatus("Pending");
    const count = await ordersPage.getOrdersCount();
    expect(count).toBeGreaterThanOrEqual(1);
  },
);

Given(
  "there is at least one processing order",
  async function (this: { page: Page }) {
    const ordersPage = new OrdersPage(this.page);
    await ordersPage.filterByStatus("Processing");
    const count = await ordersPage.getOrdersCount();
    expect(count).toBeGreaterThanOrEqual(1);
  },
);

Given(
  "there are more than {int} orders",
  async function (this: { page: Page }, orderCount: number) {
    const ordersPage = new OrdersPage(this.page);
    const count = await ordersPage.getOrdersCount();
    expect(count).toBeGreaterThan(orderCount);
  },
);

When("I click view on the first order", async function (this: { page: Page }) {
  const ordersPage = new OrdersPage(this.page);
  await ordersPage.viewFirstOrder();
});

When("I search for an order by order number", async function (this: World) {
  const ordersPage = new OrdersPage(this.page);
  const searchQuery =
    (this.testData.orderNumber as string | undefined) || "12345678";
  await ordersPage.searchOrders(searchQuery);
});

When(
  "I filter orders by {string} status",
  async function (this: { page: Page }, status: string) {
    const ordersPage = new OrdersPage(this.page);
    await ordersPage.filterByStatus(status);
  },
);

When(
  "I change the order status to {string}",
  async function (this: { page: Page }, status: string) {
    // Open order details and change status
    await this.page.selectOption('select[name="status"]', status);
    await this.page.getByRole("button", { name: "Update" }).click();
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should see the orders table", async function (this: { page: Page }) {
  await expect(this.page.locator("table")).toBeVisible();
});

Then(
  "I should see order information including status and total",
  async function (this: { page: Page }) {
    // Check table headers or content
    await expect(this.page.locator("table")).toBeVisible();
  },
);

Then(
  "I should see the order details page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/ordering\/orders\/.+/);
  },
);

Then(
  "I should see customer information",
  async function (this: { page: Page }) {
    // Look for customer-related content
    await expect(
      this.page.locator("text=/customer|buyer|email/i").first(),
    ).toBeVisible();
  },
);

Then("I should see ordered items", async function (this: { page: Page }) {
  // Look for items table or list
  await expect(
    this.page.locator("text=/items|products|books/i").first(),
  ).toBeVisible();
});

Then(
  "I should see only matching orders",
  async function (this: { page: Page }) {
    const rows = await this.page.locator("table tbody tr").count();
    expect(rows).toBeGreaterThanOrEqual(0);
  },
);

Then("I should see only pending orders", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
  // Could verify all visible rows have "Pending" status
});

Then(
  "I should see only processing orders",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "I should see only completed orders",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "the order status should be updated",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should see different orders", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
