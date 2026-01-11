import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Login Page Object
 * Handles authentication-related interactions
 */
export class LoginPage extends BasePage {
  // Selectors
  private readonly emailInput = 'input[name="email"]';
  private readonly passwordInput = 'input[name="password"]';
  private readonly loginButton = 'button[type="submit"]';
  private readonly errorMessage = '[role="alert"]';
  private readonly loginHeading = 'h1:has-text("Sign in")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to login page
   */
  async navigate(): Promise<void> {
    await this.goto("/api/auth/signin");
  }

  /**
   * Perform login with credentials
   */
  async login(email: string, password: string): Promise<void> {
    await this.fill(this.emailInput, email);
    await this.fill(this.passwordInput, password);
    await this.click(this.loginButton);
    await this.waitForNavigation();
  }

  /**
   * Fill email field
   */
  async fillEmail(email: string): Promise<void> {
    await this.fill(this.emailInput, email);
  }

  /**
   * Fill password field
   */
  async fillPassword(password: string): Promise<void> {
    await this.fill(this.passwordInput, password);
  }

  /**
   * Click login button
   */
  async clickLogin(): Promise<void> {
    await this.click(this.loginButton);
  }

  /**
   * Check if error message is displayed
   */
  async hasErrorMessage(): Promise<boolean> {
    return await this.isVisible(this.errorMessage);
  }

  /**
   * Get error message text
   */
  async getErrorMessage(): Promise<string> {
    return await this.getText(this.errorMessage);
  }

  /**
   * Assert login page is displayed
   */
  async assertIsLoginPage(): Promise<void> {
    await this.assertVisible(this.loginHeading);
    await this.assertVisible(this.emailInput);
    await this.assertVisible(this.passwordInput);
  }

  /**
   * Wait for redirect to external OAuth provider
   */
  async waitForOAuthRedirect(): Promise<void> {
    await this.waitForUrl(/keycloak|oauth|auth/, 10000);
  }
}
