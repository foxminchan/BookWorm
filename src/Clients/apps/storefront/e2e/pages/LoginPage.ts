import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Login Page Object Model
 * Represents the login page at '/login'
 */
export class LoginPage extends BasePage {
  readonly path = "/login";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get redirectingMessage(): Locator {
    return this.page.locator('h1:has-text("Redirecting to login")');
  }

  get loadingSpinner(): Locator {
    return this.page.locator('[class*="animate-spin"]');
  }

  get redirectDescription(): Locator {
    return this.page.locator('p:has-text("redirect you to the login page")');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async waitForKeycloakRedirect(): Promise<void> {
    // Wait for redirect to Keycloak OAuth provider
    await this.page.waitForURL(/keycloak|auth/, { timeout: 10000 });
  }

  // Assertions
  async isOnLoginPage(): Promise<boolean> {
    return this.page.url().includes("/login");
  }
}
