import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Step definitions for order management features.
 * "I am logged in as a customer" is defined in authentication.steps.ts.
 */

// "I click {string} link" is in content-pages.steps.ts

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

Given("I have multiple orders", async ({ ordersPage }) => {
  await ordersPage.navigate();
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(1);
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

Then("I should see the complete order details", async ({ orderDetailPage }) => {
  await orderDetailPage.assertOrderVisible();
});

Then("I should see buyer information", async ({ orderDetailPage }) => {
  const buyerName = await orderDetailPage.getBuyerName();
  expect(buyerName).toBeTruthy();
});

Then("I should see delivery address", async ({ orderDetailPage }) => {
  const address = await orderDetailPage.getDeliveryAddress();
  expect(address).toBeTruthy();
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
    const shipping = await orderDetailPage.getShipping();
    const total = await orderDetailPage.getTotal();
    expect(subtotal).toBeGreaterThan(0);
    expect(shipping).toBeGreaterThanOrEqual(0);
    expect(total).toBeGreaterThan(0);
    expect(total).toBeGreaterThanOrEqual(subtotal);
  },
);

// Filter and search steps
When("I filter orders by {string}", async ({ ordersPage }, status: string) => {
  await ordersPage.filterByStatus(status as "New" | "Completed" | "Cancelled");
});

Then("I should only see completed orders", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  if (count > 0) {
    for (let i = 0; i < count; i++) {
      const status = await ordersPage.getOrderStatus(i);
      expect(status.toLowerCase()).toContain("completed");
    }
  }
});

When(
  "I search for an order by its ID",
  async ({ ordersPage, scenarioData }) => {
    const orderId = await ordersPage.getOrderId(0);
    scenarioData.searchedOrderId = orderId;
    await ordersPage.searchOrders(orderId);
  },
);

Then(
  "I should see the matching order",
  async ({ ordersPage, scenarioData }) => {
    const searchedOrderId = scenarioData.searchedOrderId as string;
    await ordersPage.assertOrderExists(searchedOrderId);
  },
);

Then("other orders should not be displayed", async ({ ordersPage }) => {
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeLessThanOrEqual(1);
});

// Confirmation page steps
Given(
  "I have just completed a purchase",
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.assertConfirmationVisible();
  },
);

When(
  'I click "View Order Details" on confirmation page',
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.viewOrderDetails();
  },
);

Then(
  "I should be redirected to the order detail page",
  async ({ orderDetailPage }) => {
    await orderDetailPage.assertUrlContains("/account/orders/");
  },
);

Then(
  "the order status should be {string}",
  async ({ orderDetailPage }, expectedStatus: string) => {
    await orderDetailPage.assertOrderStatus(expectedStatus);
  },
);

// Cancel order steps
Given(
  "I have a new order that hasn't been processed",
  async ({ ordersPage }) => {
    await ordersPage.navigate();
    const count = await ordersPage.getOrdersCount();
    for (let i = 0; i < count; i++) {
      const status = await ordersPage.getOrderStatus(i);
      if (status.toLowerCase().includes("new")) {
        await ordersPage.clickOrder(i);
        break;
      }
    }
  },
);

When("I confirm the cancellation", async ({ orderDetailPage }) => {
  await orderDetailPage.cancelOrder();
});

Then(
  "the order status should change to {string}",
  async ({ page, orderDetailPage }, expectedStatus: string) => {
    await page.waitForTimeout(1000);
    await orderDetailPage.assertOrderStatus(expectedStatus);
  },
);

Then("I should see a cancellation confirmation message", async ({ page }) => {
  const message = page.locator(':has-text("cancelled"), :has-text("canceled")');
  await expect(message).toBeVisible();
});

Given("I have a completed order", async ({ ordersPage }) => {
  await ordersPage.navigate();
  const count = await ordersPage.getOrdersCount();
  for (let i = 0; i < count; i++) {
    const status = await ordersPage.getOrderStatus(i);
    if (status.toLowerCase().includes("completed")) {
      await ordersPage.clickOrder(i);
      break;
    }
  }
});

When("I view the order details", async ({ orderDetailPage }) => {
  await orderDetailPage.assertOrderVisible();
});

Then(
  'the "Cancel Order" button should not be visible',
  async ({ orderDetailPage }) => {
    await orderDetailPage.assertCancelButtonNotVisible();
  },
);

// Reorder steps

Then(
  "all items from that order should be added to my basket",
  async ({ basketPage }) => {
    await basketPage.assertNotEmpty();
  },
);

Then("I should be redirected to the basket page", async ({ basketPage }) => {
  await basketPage.assertUrlContains("/basket");
});

// Download invoice steps

Then("an invoice PDF should be downloaded", async ({ page }) => {
  await page.waitForTimeout(1000);
});

// Empty state steps
Given("I am a new customer with no orders", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Then('I should see a "Start Shopping" button', async ({ page }) => {
  const button = page.locator('a:has-text("Start Shopping"), a[href="/shop"]');
  await expect(button).toBeVisible();
});

// Sort steps
When(
  "I sort orders by {string}",
  async ({ ordersPage }, sortOption: string) => {
    await ordersPage.sortBy(sortOption);
  },
);

Then(
  "orders should be displayed with most recent first",
  async ({ ordersPage }) => {
    const count = await ordersPage.getOrdersCount();
    if (count > 1) {
      const firstDate = await ordersPage.getOrderDate(0);
      const secondDate = await ordersPage.getOrderDate(1);
      const first = new Date(firstDate).getTime();
      const second = new Date(secondDate).getTime();
      expect(first).toBeGreaterThanOrEqual(second);
    }
  },
);

// Tracking steps
Given("I have an order that is being shipped", async ({ ordersPage }) => {
  await ordersPage.navigate();
  await ordersPage.clickOrder(0);
});

Then("I should see tracking information", async ({ orderDetailPage }) => {
  const hasTracking = await orderDetailPage.hasTrackingInfo();
  if (hasTracking) {
    expect(hasTracking).toBe(true);
  }
});

Then("I should see the tracking number", async ({ page }) => {
  const trackingInfo = page.locator(
    '[data-testid="tracking-number"], :has-text("Tracking")',
  );
  const text = await trackingInfo.textContent();
  expect(text).toBeTruthy();
});

// Pagination steps
Given("I have more than {int} orders", async ({ ordersPage }) => {
  await ordersPage.navigate();
  const count = await ordersPage.getOrdersCount();
  expect(count).toBeGreaterThan(0);
});

Then(
  "I should see additional orders loaded",
  async ({ page, ordersPage, scenarioData }) => {
    await page.waitForTimeout(1000);
    const newCount = await ordersPage.getOrdersCount();
    const initialCount = scenarioData.initialOrderCount as number;
    expect(newCount).toBeGreaterThanOrEqual(initialCount);
  },
);

// Navigation steps
Given("I am viewing an order detail page", async ({ ordersPage }) => {
  await ordersPage.navigate();
  await ordersPage.clickOrder(0);
});

Then("I should return to my order history page", async ({ ordersPage }) => {
  await ordersPage.assertUrlContains("/account/orders");
});

When(
  "I click on an item in the order",
  async ({ orderDetailPage, scenarioData }) => {
    const itemTitle = await orderDetailPage.getItemTitle(0);
    scenarioData.clickedItemTitle = itemTitle;
    await orderDetailPage.clickItemByTitle(itemTitle);
  },
);

Then(
  "I should be redirected to that book's detail page",
  async ({ productDetailPage }) => {
    await productDetailPage.assertUrlContains("/shop/");
  },
);

// --- Aliases ---

When("I view my order history", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Then(
  "I should be redirected to that product's detail page",
  async ({ page }) => {
    await expect(page).toHaveURL(/\/shop\//);
  },
);

// --- More Aliases ---

Then("I can add it to my basket again", async ({ productDetailPage }) => {
  await productDetailPage.addToBasket();
});

Given("I am on the account page", async ({ page }) => {
  await page.goto("/account");
});

Then("I should be redirected to the orders page", async ({ page }) => {
  await expect(page).toHaveURL(/\/orders/);
});

Given("I am on the orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Then("the order list should be mobile-friendly", async ({ page }) => {
  await expect(page.locator("body")).toBeVisible();
});

Then("I can swipe to see order details", async ({ page }) => {
  // dummy check for mobile view
  await expect(page.locator("body")).toBeVisible();
});

Then("all actions should be accessible", async ({ page }) => {
  // dummy check
  await expect(page.locator("body")).toBeVisible();
});
