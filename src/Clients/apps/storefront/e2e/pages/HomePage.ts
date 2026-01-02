import { Locator, Page } from "@playwright/test";
import { expect } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Home Page Object Model
 * Represents the main landing page at '/'
 */
export class HomePage extends BasePage {
  readonly path = "/";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get heroSection(): Locator {
    return this.page
      .locator('[data-testid="hero-section"], section:has-text("Welcome")')
      .first();
  }

  get browseCollectionButton(): Locator {
    return this.page.locator(
      'a:has-text("Browse Collection"), a[href="/shop"]',
    );
  }

  get featuredBooksSection(): Locator {
    return this.page.locator(
      '[data-testid="featured-books"], section:has-text("Featured")',
    );
  }

  get featuredBookCards(): Locator {
    return this.featuredBooksSection.locator(
      '[data-testid="book-card"], article, .book-card',
    );
  }

  get categoriesSection(): Locator {
    return this.page.locator(
      '[data-testid="categories-section"], section:has-text("Categories")',
    );
  }

  get categoryCards(): Locator {
    return this.categoriesSection.locator(
      'a[href*="/shop?category="], [data-testid="category-card"]',
    );
  }

  get viewAllCategoriesLink(): Locator {
    return this.page.locator('a[href="/categories"]');
  }

  get viewAllPublishersLink(): Locator {
    return this.page.locator('a[href="/publishers"]');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async clickBrowseCollection(): Promise<void> {
    await this.browseCollectionButton.click();
    await this.waitForPageLoad();
  }

  async clickCategory(categoryName: string): Promise<void> {
    const category = this.page.locator(`a:has-text("${categoryName}")`).first();
    await category.click();
    await this.waitForPageLoad();
  }

  async clickFeaturedBook(index: number = 0): Promise<void> {
    const cards = await this.featuredBookCards.all();
    if (cards.length > index) {
      await cards[index]!.click();
      await this.waitForPageLoad();
    } else {
      throw new Error(
        `Featured book at index ${index} not found. Total cards: ${cards.length}`,
      );
    }
  }

  async getFeaturedBooksCount(): Promise<number> {
    return await this.featuredBookCards.count();
  }

  async getCategoriesCount(): Promise<number> {
    return await this.categoryCards.count();
  }

  async viewAllCategories(): Promise<void> {
    await this.viewAllCategoriesLink.click();
    await this.waitForPageLoad();
  }

  async viewAllPublishers(): Promise<void> {
    await this.viewAllPublishersLink.click();
    await this.waitForPageLoad();
  }

  // Assertions
  async assertHeroVisible(): Promise<void> {
    await expect(this.heroSection).toBeVisible();
  }

  async assertFeaturedBooksVisible(): Promise<void> {
    await expect(this.featuredBooksSection).toBeVisible();
    const count = await this.getFeaturedBooksCount();
    expect(count).toBeGreaterThan(0);
  }

  async assertCategoriesVisible(): Promise<void> {
    await expect(this.categoriesSection).toBeVisible();
    const count = await this.getCategoriesCount();
    expect(count).toBeGreaterThan(0);
  }
}
