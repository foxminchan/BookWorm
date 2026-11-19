import { Page, expect } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Auth Guard / Entra ID OAuth Page Object
 * Backoffice uses better-auth with Microsoft Entra ID OAuth via AuthGuard.
 */
export class LoginPage extends BasePage {
  private readonly authStatusHeading = 'h1:has-text("Authentication Required")';
  private readonly authRedirectDescription =
    'text="Redirecting to secure login..."';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Assert AuthGuard authentication required card is displayed
   */
  async assertAuthRequiredDisplayed(): Promise<void> {
    await expect(this.page.locator(this.authStatusHeading)).toBeVisible({
      timeout: 15000,
    });
    await expect(this.page.locator(this.authRedirectDescription)).toBeVisible();
  }

  /**
   * Wait for redirect to Microsoft Entra ID OAuth provider or social sign-in call
   */
  async waitForOAuthRedirect(): Promise<void> {
    await this.waitForUrl(
      /login\.microsoftonline\.com|\/backoffice\/api\/auth/,
      10000,
    );
  }
}
