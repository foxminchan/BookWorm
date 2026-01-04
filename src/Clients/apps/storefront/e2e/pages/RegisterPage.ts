import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Register Page Object Model
 * Represents the registration page at '/register'
 */
export class RegisterPage extends BasePage {
  readonly path = "/register";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get redirectingMessage(): Locator {
    return this.page.locator('h1:has-text("Redirecting to registration")');
  }

  get loadingSpinner(): Locator {
    return this.page.locator('[class*="animate-spin"]');
  }

  get redirectDescription(): Locator {
    return this.page.locator(
      'p:has-text("redirect you to the registration page")',
    );
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
  async isOnRegisterPage(): Promise<boolean> {
    return this.page.url().includes("/register");
  }
}
