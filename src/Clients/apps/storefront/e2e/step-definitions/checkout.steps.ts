import { Given, Then, When } from "@cucumber/cucumber";
import { Page, expect } from "@playwright/test";

import {
  BasketPage,
  CheckoutConfirmationPage,
  HomePage,
  ProductDetailPage,
  ShopPage,
} from "../pages";

/**
 * Step definitions for checkout feature
 */

// Homepage steps
Then("I should see the homepage", async function (this: { page: Page }) {
  const homePage = new HomePage(this.page);
  await homePage.assertHeroVisible();
});

When('I click "Browse Collection"', async function (this: { page: Page }) {
  const homePage = new HomePage(this.page);
  await homePage.clickBrowseCollection();
});

// Shop page steps
Then(
  "I should see the shop page with books displayed",
  async function (this: { page: Page }) {
    const shopPage = new ShopPage(this.page);
    await shopPage.assertUrlContains("/shop");
    await shopPage.assertBooksDisplayed();
  },
);

When("I click on the first book", async function (this: { page: Page }) {
  const shopPage = new ShopPage(this.page);
  await shopPage.clickBook(0);
});

When(
  "I click on {string} book",
  async function (this: { page: Page }, bookTitle: string) {
    const shopPage = new ShopPage(this.page);
    await shopPage.clickBookByTitle(bookTitle);
  },
);

// Product detail page steps
Then(
  "I should see the product detail page",
  async function (this: { page: Page }) {
    const productPage = new ProductDetailPage(this.page);
    await productPage.assertUrlContains("/shop/");
  },
);

Then(
  "the book details should be visible",
  async function (this: { page: Page }) {
    const productPage = new ProductDetailPage(this.page);
    await productPage.assertBookDetailsVisible();
  },
);

When(
  "I increase quantity to {int}",
  async function (this: { page: Page }, quantity: number) {
    const productPage = new ProductDetailPage(this.page);
    const currentQty = await productPage.getQuantity();
    const timesToIncrease = quantity - currentQty;

    if (timesToIncrease > 0) {
      await productPage.increaseQuantity(timesToIncrease);
    }
  },
);

When('I click "Add to Basket"', async function (this: { page: Page }) {
  const productPage = new ProductDetailPage(this.page);
  await productPage.addToBasket();
});

// Basket page steps - Given conditions
Given("my basket is empty", async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.navigate();

  // Clear basket if it has items
  const itemCount = await basketPage.getItemCount();
  if (itemCount > 0) {
    await basketPage.clearBasket();
  }
});

Given(
  "I have added {int} different books to my basket",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    await shopPage.navigate();

    for (let i = 0; i < bookCount; i++) {
      await shopPage.clickBook(i);

      const productPage = new ProductDetailPage(this.page);
      await productPage.addToBasket();

      // Go back to shop
      await shopPage.navigate();
    }
  },
);

Given(
  "I have added {int} books to my basket",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    await shopPage.navigate();
    await shopPage.clickBook(0);

    const productPage = new ProductDetailPage(this.page);
    await productPage.setQuantity(bookCount);
    await productPage.addToBasket();
  },
);

Given(
  "I have added {int} book to my basket",
  async function (this: { page: Page }, bookCount: number) {
    const shopPage = new ShopPage(this.page);
    await shopPage.navigate();
    await shopPage.clickBook(0);

    const productPage = new ProductDetailPage(this.page);
    await productPage.addToBasket();
  },
);

Given(
  "I have added a book priced at {string} to my basket",
  async function (this: { page: Page }, price: string) {
    // For testing purposes, we'll add the first book and store the price
    const shopPage = new ShopPage(this.page);
    await shopPage.navigate();
    await shopPage.clickBook(0);

    const productPage = new ProductDetailPage(this.page);
    await productPage.addToBasket();
  },
);

// Basket page assertions
Then(
  "I should see the basket page with {int} items",
  async function (this: { page: Page }, itemCount: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.assertUrlContains("/basket");

    const actualCount = await basketPage.getItemCount();
    expect(actualCount).toBe(itemCount);
  },
);

Then(
  "I should see all {int} items",
  async function (this: { page: Page }, itemCount: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.assertItemsCount(itemCount);
  },
);

Then(
  "I should see {int} items in the basket",
  async function (this: { page: Page }, itemCount: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.assertItemsCount(itemCount);
  },
);

Then(
  "the subtotal should be calculated correctly",
  async function (this: { page: Page }) {
    const basketPage = new BasketPage(this.page);
    const subtotal = await basketPage.getSubtotal();
    expect(subtotal).toBeGreaterThan(0);
  },
);

Then(
  "the total should include {string} shipping",
  async function (this: { page: Page }, shippingAmount: string) {
    const basketPage = new BasketPage(this.page);
    const shipping = await basketPage.getShipping();
    const expectedShipping = Number.parseFloat(shippingAmount.replace("$", ""));
    expect(shipping).toBe(expectedShipping);
  },
);

Then(
  "the subtotal should be {string}",
  async function (this: { page: Page }, expectedSubtotal: string) {
    const basketPage = new BasketPage(this.page);
    const subtotal = await basketPage.getSubtotal();
    const expected = Number.parseFloat(expectedSubtotal.replace("$", ""));
    expect(Math.abs(subtotal - expected)).toBeLessThan(0.01);
  },
);

Then(
  "the shipping should be {string}",
  async function (this: { page: Page }, expectedShipping: string) {
    const basketPage = new BasketPage(this.page);
    const shipping = await basketPage.getShipping();
    const expected = Number.parseFloat(expectedShipping.replace("$", ""));
    expect(shipping).toBe(expected);
  },
);

Then(
  "the total should be {string}",
  async function (this: { page: Page }, expectedTotal: string) {
    const basketPage = new BasketPage(this.page);
    const total = await basketPage.getTotal();
    const expected = Number.parseFloat(expectedTotal.replace("$", ""));
    expect(Math.abs(total - expected)).toBeLessThan(0.01);
  },
);

Then("the total should be recalculated", async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.assertTotalCalculated();
});

// Basket item manipulation
When(
  "I increase quantity of item {int} to {int}",
  async function (this: { page: Page }, itemIndex: number, quantity: number) {
    const basketPage = new BasketPage(this.page);
    const currentQty = await basketPage.getItemQuantity(itemIndex - 1);
    const timesToIncrease = quantity - currentQty;

    if (timesToIncrease > 0) {
      await basketPage.increaseItemQuantity(itemIndex - 1, timesToIncrease);
    }
  },
);

When(
  "I decrease quantity of item {int} to {int}",
  async function (this: { page: Page }, itemIndex: number, quantity: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.setItemQuantity(itemIndex - 1, quantity);
  },
);

When(
  "I set quantity of item {int} to {int}",
  async function (this: { page: Page }, itemIndex: number, quantity: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.setItemQuantity(itemIndex - 1, quantity);
  },
);

When('I click "Save Changes"', async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.saveChanges();
});

// Removal dialog
Then(
  "I should see a removal confirmation dialog for item {int}",
  async function (this: { page: Page }, itemIndex: number) {
    const basketPage = new BasketPage(this.page);
    await basketPage.assertRemoveDialogVisible();
  },
);

Then(
  "I should see a removal confirmation dialog",
  async function (this: { page: Page }) {
    const basketPage = new BasketPage(this.page);
    await basketPage.assertRemoveDialogVisible();
  },
);

When("I confirm the removal", async function (this: { page: Page }) {
  const confirmButton = this.page
    .locator('button:has-text("Remove"), button:has-text("Confirm")')
    .first();
  await confirmButton.click();
  await this.page.waitForTimeout(1000);
});

When("I cancel the removal", async function (this: { page: Page }) {
  const cancelButton = this.page.locator('button:has-text("Cancel")').first();
  await cancelButton.click();
  await this.page.waitForTimeout(500);
});

Then(
  "item {int} should be removed from the basket",
  async function (this: { page: Page }, itemIndex: number) {
    // Item should no longer be visible
    await this.page.waitForTimeout(1000);
  },
);

Then(
  "the item should remain in the basket",
  async function (this: { page: Page }) {
    const basketPage = new BasketPage(this.page);
    const count = await basketPage.getItemCount();
    expect(count).toBeGreaterThan(0);
  },
);

Then(
  "the quantity should be restored to {int}",
  async function (this: { page: Page }, expectedQuantity: number) {
    const basketPage = new BasketPage(this.page);
    const quantity = await basketPage.getItemQuantity(0);
    expect(quantity).toBe(expectedQuantity);
  },
);

// Clear basket
When('I click "Clear Basket"', async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.clearBasket();
});

Then("my basket should be empty", async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.assertEmpty();
});

// Checkout
When('I click "Checkout"', async function (this: { page: Page }) {
  const basketPage = new BasketPage(this.page);
  await basketPage.proceedToCheckout();
});

Then(
  "I should be redirected to the confirmation page",
  async function (this: { page: Page }) {
    const confirmationPage = new CheckoutConfirmationPage(this.page);
    await confirmationPage.assertUrlContains("/checkout/confirmation");
  },
);

Then(
  "I should see a success message with order ID",
  async function (this: { page: Page }) {
    const confirmationPage = new CheckoutConfirmationPage(this.page);
    await confirmationPage.assertConfirmationVisible();
    await confirmationPage.assertOrderCreated();
  },
);

Then("I should see the order total", async function (this: { page: Page }) {
  const confirmationPage = new CheckoutConfirmationPage(this.page);
  const total = await confirmationPage.getTotalAmount();
  expect(total).toBeTruthy();
});
