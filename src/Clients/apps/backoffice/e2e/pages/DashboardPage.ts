import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Dashboard/Home Page Object
 * Main landing page after successful login
 */
export class DashboardPage extends BasePage {
  // Selectors
  private readonly dashboardHeading = 'h1:has-text("Dashboard")';
  private readonly statsCards = '[data-testid="stat-card"]';
  private readonly navigationMenu = "nav";
  private readonly catalogLink = 'a:has-text("Catalog")';
  private readonly ordersLink = 'a:has-text("Orders")';
  private readonly customersLink = 'a:has-text("Customers")';
  private readonly reviewsLink = 'a:has-text("Reviews")';
  private readonly userMenu = '[data-testid="user-menu"]';
  private readonly logoutButton = 'button:has-text("Logout")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to dashboard
   */
  async navigate(): Promise<void> {
    await this.goto("/");
  }

  /**
   * Assert dashboard page is loaded
   */
  async assertIsDashboardPage(): Promise<void> {
    await this.assertVisible(this.navigationMenu);
    await this.waitForPageLoad();
  }

  /**
   * Navigate to catalog section
   */
  async navigateToCatalog(): Promise<void> {
    await this.click(this.catalogLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to orders section
   */
  async navigateToOrders(): Promise<void> {
    await this.click(this.ordersLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to customers section
   */
  async navigateToCustomers(): Promise<void> {
    await this.click(this.customersLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to reviews section
   */
  async navigateToReviews(): Promise<void> {
    await this.click(this.reviewsLink);
    await this.waitForNavigation();
  }

  /**
   * Open user menu
   */
  async openUserMenu(): Promise<void> {
    await this.click(this.userMenu);
  }

  /**
   * Logout from application
   */
  async logout(): Promise<void> {
    await this.openUserMenu();
    await this.click(this.logoutButton);
    await this.waitForNavigation();
  }

  /**
   * Get dashboard statistics count
   */
  async getStatsCount(): Promise<number> {
    const cards = await this.page.locator(this.statsCards).count();
    return cards;
  }

  /**
   * Check if navigation menu is visible
   */
  async isNavigationVisible(): Promise<boolean> {
    return await this.isVisible(this.navigationMenu);
  }
}
