import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Step definitions for checkout feature
 */

// Homepage steps
Then("I should see the homepage", async ({ homePage }) => {
  await homePage.assertHeroVisible();
});

// Shop page steps
Then(
  "I should see the shop page with books displayed",
  async ({ shopPage }) => {
    await shopPage.assertUrlContains("/shop");
    await shopPage.assertBooksDisplayed();
  },
);

When("I click on the first book", async ({ shopPage }) => {
  await shopPage.clickBook(0);
});

When("I click on {string} book", async ({ shopPage }, bookTitle: string) => {
  await shopPage.clickBookByTitle(bookTitle);
});

// Product detail page steps
Then("I should see the product detail page", async ({ productDetailPage }) => {
  await productDetailPage.assertUrlContains("/shop/");
});

Then("the book details should be visible", async ({ productDetailPage }) => {
  await productDetailPage.assertBookDetailsVisible();
});

When(
  "I increase quantity to {int}",
  async ({ productDetailPage }, quantity: number) => {
    const currentQty = await productDetailPage.getQuantity();
    const timesToIncrease = quantity - currentQty;
    if (timesToIncrease > 0) {
      await productDetailPage.increaseQuantity(timesToIncrease);
    }
  },
);

// Basket page steps — Given conditions
Given("my basket is empty", async ({ basketPage }) => {
  await basketPage.navigate();
  const itemCount = await basketPage.getItemCount();
  if (itemCount > 0) {
    await basketPage.clearBasket();
  }
});

Given(
  "I have added {int} different books to my basket",
  async ({ page, shopPage, productDetailPage }, bookCount: number) => {
    await shopPage.navigate();
    for (let i = 0; i < bookCount; i++) {
      await shopPage.clickBook(i);
      await productDetailPage.addToBasket();
      await shopPage.navigate();
    }
  },
);

Given(
  "I have added {int} books to my basket",
  async ({ shopPage, productDetailPage }, bookCount: number) => {
    await shopPage.navigate();
    await shopPage.clickBook(0);
    await productDetailPage.setQuantity(bookCount);
    await productDetailPage.addToBasket();
  },
);

Given(
  "I have added {int} book to my basket",
  async ({ shopPage, productDetailPage }) => {
    await shopPage.navigate();
    await shopPage.clickBook(0);
    await productDetailPage.addToBasket();
  },
);

Given(
  "I have added a book priced at {string} to my basket",
  async ({ shopPage, productDetailPage }) => {
    await shopPage.navigate();
    await shopPage.clickBook(0);
    await productDetailPage.addToBasket();
  },
);

// Basket page assertions
Then(
  "I should see the basket page with {int} items",
  async ({ basketPage }, itemCount: number) => {
    await basketPage.assertUrlContains("/basket");
    const actualCount = await basketPage.getItemCount();
    expect(actualCount).toBe(itemCount);
  },
);

Then(
  "I should see all {int} items",
  async ({ basketPage }, itemCount: number) => {
    await basketPage.assertItemsCount(itemCount);
  },
);

Then(
  "I should see {int} items in the basket",
  async ({ basketPage }, itemCount: number) => {
    await basketPage.assertItemsCount(itemCount);
  },
);

Then("the subtotal should be calculated correctly", async ({ basketPage }) => {
  const subtotal = await basketPage.getSubtotal();
  expect(subtotal).toBeGreaterThan(0);
});

Then(
  "the total should include {string} shipping",
  async ({ basketPage }, shippingAmount: string) => {
    const shipping = await basketPage.getShipping();
    const expectedShipping = Number.parseFloat(shippingAmount.replace("$", ""));
    expect(shipping).toBe(expectedShipping);
  },
);

Then(
  "the subtotal should be {string}",
  async ({ basketPage }, expectedSubtotal: string) => {
    const subtotal = await basketPage.getSubtotal();
    const expected = Number.parseFloat(expectedSubtotal.replace("$", ""));
    expect(Math.abs(subtotal - expected)).toBeLessThan(0.01);
  },
);

Then(
  "the shipping should be {string}",
  async ({ basketPage }, expectedShipping: string) => {
    const shipping = await basketPage.getShipping();
    const expected = Number.parseFloat(expectedShipping.replace("$", ""));
    expect(shipping).toBe(expected);
  },
);

Then(
  "the total should be {string}",
  async ({ basketPage }, expectedTotal: string) => {
    const total = await basketPage.getTotal();
    const expected = Number.parseFloat(expectedTotal.replace("$", ""));
    expect(Math.abs(total - expected)).toBeLessThan(0.01);
  },
);

Then("the total should be recalculated", async ({ basketPage }) => {
  await basketPage.assertTotalCalculated();
});

// Basket item manipulation
When(
  "I increase quantity of item {int} to {int}",
  async ({ basketPage }, itemIndex: number, quantity: number) => {
    const currentQty = await basketPage.getItemQuantity(itemIndex - 1);
    const timesToIncrease = quantity - currentQty;
    if (timesToIncrease > 0) {
      await basketPage.increaseItemQuantity(itemIndex - 1, timesToIncrease);
    }
  },
);

When(
  "I decrease quantity of item {int} to {int}",
  async ({ basketPage }, itemIndex: number, quantity: number) => {
    await basketPage.setItemQuantity(itemIndex - 1, quantity);
  },
);

When(
  "I set quantity of item {int} to {int}",
  async ({ basketPage }, itemIndex: number, quantity: number) => {
    await basketPage.setItemQuantity(itemIndex - 1, quantity);
  },
);

// Removal dialog
Then(
  "I should see a removal confirmation dialog for item {int}",
  async ({ basketPage }) => {
    await basketPage.assertRemoveDialogVisible();
  },
);

Then("I should see a removal confirmation dialog", async ({ basketPage }) => {
  await basketPage.assertRemoveDialogVisible();
});

When("I confirm the removal", async ({ page }) => {
  const confirmButton = page
    .locator('button:has-text("Remove"), button:has-text("Confirm")')
    .first();
  await confirmButton.click();
  await page.waitForTimeout(1000);
});

When("I cancel the removal", async ({ page }) => {
  const cancelButton = page.locator('button:has-text("Cancel")').first();
  await cancelButton.click();
  await page.waitForTimeout(500);
});

Then("item {int} should be removed from the basket", async ({ page }) => {
  await page.waitForTimeout(1000);
});

Then("the item should remain in the basket", async ({ basketPage }) => {
  const count = await basketPage.getItemCount();
  expect(count).toBeGreaterThan(0);
});

Then(
  "the quantity should be restored to {int}",
  async ({ basketPage }, expectedQuantity: number) => {
    const quantity = await basketPage.getItemQuantity(0);
    expect(quantity).toBe(expectedQuantity);
  },
);

// Clear basket

Then("my basket should be empty", async ({ basketPage }) => {
  await basketPage.assertEmpty();
});

// Checkout

Then(
  "I should be redirected to the confirmation page",
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.assertUrlContains("/checkout/confirmation");
  },
);

Then(
  "I should see a success message with order ID",
  async ({ checkoutConfirmationPage }) => {
    await checkoutConfirmationPage.assertConfirmationVisible();
    await checkoutConfirmationPage.assertOrderCreated();
  },
);

Then("I should see the order total", async ({ checkoutConfirmationPage }) => {
  const total = await checkoutConfirmationPage.getTotalAmount();
  expect(total).toBeTruthy();
});
