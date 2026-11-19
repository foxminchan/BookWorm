import { Locator, Page, expect } from "@playwright/test";

import { forEachSequentially } from "../utils/forEachSequentially";
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
    return this.page.locator('main [role="article"]');
  }

  get emptyState(): Locator {
    return this.page.locator(':has-text("Your basket is empty")').first();
  }

  get exploreCollectionLink(): Locator {
    return this.page.locator(
      'a:has-text("Explore the Collection"), a[href="/shop"]',
    );
  }

  get subtotalAmount(): Locator {
    return this.page.locator('div:has(> span:text-is("Subtotal"))').first();
  }

  get shippingAmount(): Locator {
    return this.page.locator('div:has(> span:text-is("Shipping"))').first();
  }

  get totalAmount(): Locator {
    return this.page.locator('div:has(> span:text-is("Total"))').first();
  }

  get checkoutButton(): Locator {
    return this.page.locator('button:has-text("Checkout")');
  }

  get clearBasketButton(): Locator {
    return this.page.locator('button:has-text("Clear Basket")');
  }

  get saveChangesButton(): Locator {
    return this.page.locator('button:has-text("Save Changes")');
  }

  get removeItemDialog(): Locator {
    return this.page.locator('[role="alertdialog"]');
  }

  get confirmRemoveButton(): Locator {
    return this.removeItemDialog.locator('button:has-text("Remove")');
  }

  get cancelRemoveButton(): Locator {
    return this.removeItemDialog.locator(
      'button:has-text("Keep in Basket"), button:has-text("Keep Item")',
    );
  }

  get confirmClearBasketButton(): Locator {
    return this.page.locator(
      '[role="alertdialog"] button:has-text("Clear Basket")',
    );
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
    const title = await item.locator("h3").textContent();
    return title?.trim() || "";
  }

  async getItemPrice(index: number): Promise<number> {
    const item = this.basketItems.nth(index);
    const priceText = await item
      .locator("div.ml-auto.text-right span.font-bold")
      .first()
      .textContent();
    const cleaned = priceText?.replaceAll(/[$,]/g, "") || "0";
    return Number.parseFloat(cleaned);
  }

  async getItemQuantity(index: number): Promise<number> {
    const item = this.basketItems.nth(index);
    const text = await item.locator("output").textContent();
    return Number.parseInt(text?.trim() || "0", 10) || 0;
  }

  async setItemQuantity(index: number, targetQuantity: number): Promise<void> {
    const current = await this.getItemQuantity(index);
    const diff = targetQuantity - current;
    if (diff > 0) {
      await this.increaseItemQuantity(index, diff);
    } else if (diff < 0) {
      await this.decreaseItemQuantity(index, Math.abs(diff));
    }
  }

  async increaseItemQuantity(index: number, times: number = 1): Promise<void> {
    const item = this.basketItems.nth(index);
    const increaseButton = item.locator(
      'button[aria-label="Increase quantity"]',
    );

    await forEachSequentially(Array.from({ length: times }), async () => {
      await increaseButton.click();
      await this.page.waitForTimeout(150);
    });
  }

  async decreaseItemQuantity(index: number, times: number = 1): Promise<void> {
    const item = this.basketItems.nth(index);
    const decreaseButton = item.locator(
      'button[aria-label="Decrease quantity"]',
    );

    await forEachSequentially(Array.from({ length: times }), async () => {
      await decreaseButton.click();
      await this.page.waitForTimeout(150);
    });
  }

  async removeItem(index: number, confirm: boolean = true): Promise<void> {
    const item = this.basketItems.nth(index);
    const removeButton = item.locator(
      'button[aria-label*="Remove"][aria-label*="from basket"]',
    );

    await removeButton.click();
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
      await expect(this.confirmClearBasketButton).toBeVisible();
      await this.confirmClearBasketButton.click();
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
    const itemCount = await this.getItemCount();
    const itemTotals = await Promise.all(
      Array.from({ length: itemCount }, async (_, index) => {
        const [price, quantity] = await Promise.all([
          this.getItemPrice(index),
          this.getItemQuantity(index),
        ]);
        return price * quantity;
      }),
    );
    const subtotal = itemTotals.reduce(
      (total, itemTotal) => total + itemTotal,
      0,
    );

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
