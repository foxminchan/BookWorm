import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Step definitions for order management features.
 * Shared steps:
 *   "I am logged in as a customer" → authentication.steps.ts
 *   "I am on my account page" → account-management.steps.ts
 *   "I click the \"Order History\" link on account page" → account-management.steps.ts
 *   "I click {string}" → common.steps.ts
 */

// Order history steps
Given("I have placed multiple orders", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Given("I have an order in my history", async ({ ordersPage }) => {
  await ordersPage.navigate();
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(0);
});

Given("I have orders with different statuses", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Given(
  "I have more than {int} orders",
  async ({ ordersPage }, _minOrders: number) => {
    await ordersPage.navigate();
  },
);

Given("I am a new customer with no orders", async ({ page }) => {
  await page.route("**/api/v1/orders*", (route) =>
    route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ items: [], totalCount: 0 }),
    }),
  );
});

When("I navigate to my orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

When("I view my order history", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Then("I should see a list of my orders", async ({ ordersPage }) => {
  await ordersPage.assertNotEmpty();
});

Then(
  "each order should display order ID, date, status, and total",
  async ({ ordersPage }) => {
    const count = await ordersPage.getOrdersCount();
    if (count > 0) {
      const orderId = await ordersPage.getOrderId(0);
      const date = await ordersPage.getOrderDate(0);
      const status = await ordersPage.getOrderStatus(0);
      const total = await ordersPage.getOrderTotal(0);
      expect(orderId).toBeTruthy();
      expect(date).toBeTruthy();
      expect(status).toBeTruthy();
      expect(total).toBeGreaterThan(0);
    }
  },
);

// Order details steps
When("I click on an order from the list", async ({ ordersPage }) => {
  await ordersPage.clickOrder(0);
});

Given("I am viewing an order detail page", async ({ ordersPage }) => {
  await ordersPage.navigate();
  await ordersPage.clickOrder(0);
});

Then("I should see the complete order details", async ({ orderDetailPage }) => {
  await orderDetailPage.assertOrderVisible();
});

Then(
  "I should see all ordered items with quantities and prices",
  async ({ orderDetailPage }) => {
    const itemsCount = await orderDetailPage.getItemsCount();
    expect(itemsCount).toBeGreaterThan(0);

    const title = await orderDetailPage.getItemTitle(0);
    const quantity = await orderDetailPage.getItemQuantity(0);
    const price = await orderDetailPage.getItemPrice(0);
    expect(title).toBeTruthy();
    expect(quantity).toBeGreaterThan(0);
    expect(price).toBeGreaterThan(0);
  },
);

Then(
  "I should see subtotal, shipping, and total amounts",
  async ({ orderDetailPage }) => {
    const subtotal = await orderDetailPage.getSubtotal();
    const shippingText = await orderDetailPage.getShippingText();
    const total = await orderDetailPage.getTotal();
    expect(subtotal).toBeGreaterThan(0);
    expect(shippingText).toContain("Free");
    expect(total).toBeGreaterThan(0);
  },
);

// Filter steps
When("I filter orders by {string}", async ({ ordersPage }, status: string) => {
  await ordersPage.filterByStatus(status);
});

Then("I should only see completed orders", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  if (count > 0) {
    const statuses = await Promise.all(
      Array.from({ length: count }, (_, index) =>
        ordersPage.getOrderStatus(index),
      ),
    );
    for (const status of statuses) {
      expect(status.toLowerCase()).toContain("completed");
    }
  }
});

// Confirmation page steps
Given(
  "I have just completed a purchase",
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.assertConfirmationVisible();
  },
);

When(
  'I click "View Order" on confirmation page',
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.viewOrderDetails();
  },
);

Then("I should be redirected to the order detail page", async ({ page }) => {
  await expect(page).toHaveURL(/\/account\/orders\/.+/);
});

Then(
  "the order status should be {string}",
  async ({ orderDetailPage }, expectedStatus: string) => {
    await orderDetailPage.assertOrderStatus(expectedStatus);
  },
);

// Pagination steps
When(
  "I click page {int} of orders",
  async ({ ordersPage }, pageNumber: number) => {
    await ordersPage.goToPage(pageNumber);
  },
);

Then("I should see the next page of orders", async ({ ordersPage }) => {
  await ordersPage.assertOrdersVisible();
});

// Navigation back
Then("I should return to my order history page", async ({ page }) => {
  await expect(page).toHaveURL(/\/account\/orders$/);
});

Then("I should be redirected to the orders page", async ({ page }) => {
  await expect(page).toHaveURL(/\/account\/orders/);
});
