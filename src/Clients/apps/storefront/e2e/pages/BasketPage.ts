import { Locator, Page, expect } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Basket Page Object Model
 * Represents the shopping basket/cart page at '/basket'
 */
export class BasketPage extends BasePage {
  readonly path = "/basket";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get basketItems(): Locator {
    return this.page.locator(
      '[data-testid="basket-item"], .basket-item, article',
    );
  }

  get emptyState(): Locator {
    return this.page.locator(
      '[data-testid="empty-basket"], :has-text("Your basket is empty")',
    );
  }

  get exploreCollectionLink(): Locator {
    return this.page.locator('a:has-text("Explore"), a[href="/shop"]');
  }

  get subtotalAmount(): Locator {
    return this.page
      .locator('[data-testid="subtotal"], :has-text("Subtotal")')
      .locator("..");
  }

  get shippingAmount(): Locator {
    return this.page
      .locator('[data-testid="shipping"], :has-text("Shipping")')
      .locator("..");
  }

  get totalAmount(): Locator {
    return this.page
      .locator('[data-testid="total"], :has-text("Total")')
      .locator("..");
  }

  get checkoutButton(): Locator {
    return this.page.locator(
      'button:has-text("Checkout"), button:has-text("Proceed to Checkout")',
    );
  }

  get clearBasketButton(): Locator {
    return this.page.locator(
      'button:has-text("Clear Basket"), button:has-text("Clear All")',
    );
  }

  get saveChangesButton(): Locator {
    return this.page.locator(
      'button:has-text("Save Changes"), button:has-text("Update Basket")',
    );
  }

  get removeItemDialog(): Locator {
    return this.page.locator('[role="dialog"], [data-testid="remove-dialog"]');
  }

  get confirmRemoveButton(): Locator {
    return this.removeItemDialog.locator(
      'button:has-text("Remove"), button:has-text("Confirm")',
    );
  }

  get cancelRemoveButton(): Locator {
    return this.removeItemDialog.locator('button:has-text("Cancel")');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async getItemCount(): Promise<number> {
    return await this.basketItems.count();
  }

  async isEmpty(): Promise<boolean> {
    try {
      await this.emptyState.waitFor({ state: "visible", timeout: 3000 });
      return true;
    } catch {
      return false;
    }
  }

  async getItemTitle(index: number): Promise<string> {
    const item = this.basketItems.nth(index);
    const title = await item
      .locator('h2, h3, [data-testid="item-title"]')
      .textContent();
    return title?.trim() || "";
  }

  async getItemPrice(index: number): Promise<number> {
    const item = this.basketItems.nth(index);
    const priceText = await item
      .locator('[data-testid="item-price"], .price')
      .textContent();
    const cleaned = priceText?.replaceAll(/[$,]/g, "") || "0";
    return Number.parseFloat(cleaned);
  }

  async getItemQuantity(index: number): Promise<number> {
    const item = this.basketItems.nth(index);
    const input = item.locator(
      'input[type="number"], [data-testid="quantity-input"]',
    );
    const value = await input.inputValue();
    return Number.parseInt(value) || 0;
  }

  async setItemQuantity(index: number, quantity: number): Promise<void> {
    const item = this.basketItems.nth(index);
    const input = item.locator(
      'input[type="number"], [data-testid="quantity-input"]',
    );
    await input.fill(quantity.toString());
  }

  async increaseItemQuantity(index: number, times: number = 1): Promise<void> {
    const item = this.basketItems.nth(index);
    const increaseButton = item.locator(
      'button[aria-label*="Increase"], button:has-text("+")',
    );

    for (let i = 0; i < times; i++) {
      await increaseButton.click();
      await this.page.waitForTimeout(200);
    }
  }

  async decreaseItemQuantity(index: number, times: number = 1): Promise<void> {
    const item = this.basketItems.nth(index);
    const decreaseButton = item.locator(
      'button[aria-label*="Decrease"], button:has-text("-")',
    );

    for (let i = 0; i < times; i++) {
      await decreaseButton.click();
      await this.page.waitForTimeout(200);
    }
  }

  async removeItem(index: number, confirm: boolean = true): Promise<void> {
    const item = this.basketItems.nth(index);
    const removeButton = item.locator(
      'button[aria-label*="Remove"][aria-label*="from basket"], button:has-text("Remove"), [data-testid="remove-button"]',
    );

    await removeButton.click();

    // Wait for dialog to appear
    await expect(this.removeItemDialog).toBeVisible();

    if (confirm) {
      await this.confirmRemoveButton.click();
    } else {
      await this.cancelRemoveButton.click();
    }

    await this.page.waitForTimeout(1000);
  }

  async saveChanges(): Promise<void> {
    await this.saveChangesButton.click();
    await this.page.waitForTimeout(1000);
  }

  async clearBasket(): Promise<void> {
    if (await this.clearBasketButton.isVisible()) {
      await this.clearBasketButton.click();
      await this.page.waitForTimeout(1000);
    }
  }

  async proceedToCheckout(): Promise<void> {
    await this.checkoutButton.click();
    await this.waitForPageLoad();
  }

  async getSubtotal(): Promise<number> {
    const text = await this.subtotalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async getShipping(): Promise<number> {
    const text = await this.shippingAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async getTotal(): Promise<number> {
    const text = await this.totalAmount.textContent();
    const match = text?.match(/\$?([\d,]+\.?\d*)/);
    return match ? Number.parseFloat(match[1]!.replaceAll(",", "")) : 0;
  }

  async calculateExpectedTotal(): Promise<number> {
    let subtotal = 0;
    const itemCount = await this.getItemCount();

    for (let i = 0; i < itemCount; i++) {
      const price = await this.getItemPrice(i);
      const quantity = await this.getItemQuantity(i);
      subtotal += price * quantity;
    }

    const shipping = await this.getShipping();
    return subtotal + shipping;
  }

  async exploreCollection(): Promise<void> {
    await this.exploreCollectionLink.click();
    await this.waitForPageLoad();
  }

  // Assertions
  async assertEmpty(): Promise<void> {
    await expect(this.emptyState).toBeVisible();
    await expect(this.checkoutButton).not.toBeVisible();
  }

  async assertNotEmpty(): Promise<void> {
    const count = await this.getItemCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertItemsCount(expectedCount: number): Promise<void> {
    const count = await this.getItemCount();
    expect(count).toBe(expectedCount);
  }

  async assertTotalCalculated(): Promise<void> {
    const displayedTotal = await this.getTotal();
    const expectedTotal = await this.calculateExpectedTotal();

    // Allow small floating point difference
    expect(Math.abs(displayedTotal - expectedTotal)).toBeLessThan(0.01);
  }

  async assertCheckoutButtonVisible(): Promise<void> {
    await expect(this.checkoutButton).toBeVisible();
    await expect(this.checkoutButton).toBeEnabled();
  }

  async assertRemoveDialogVisible(): Promise<void> {
    await expect(this.removeItemDialog).toBeVisible();
  }
}
