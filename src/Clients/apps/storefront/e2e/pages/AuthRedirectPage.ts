import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Shared Page Object Model for Entra ID auth redirect pages (/login, /register).
 */
export abstract class AuthRedirectPage extends BasePage {
  abstract readonly path: string;
  protected abstract readonly headingText: string;
  protected abstract readonly descriptionText: string;

  constructor(page: Page) {
    super(page);
  }

  get redirectingMessage(): Locator {
    return this.page.locator(`h1:has-text("${this.headingText}")`);
  }

  get loadingSpinner(): Locator {
    return this.page.locator('[class*="animate-spin"]');
  }

  get redirectDescription(): Locator {
    return this.page.locator(`p:has-text("${this.descriptionText}")`);
  }

  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async waitForEntraRedirect(): Promise<void> {
    await this.page.waitForURL(/login\.microsoftonline\.com|auth/, {
      timeout: 10000,
    });
  }
}
