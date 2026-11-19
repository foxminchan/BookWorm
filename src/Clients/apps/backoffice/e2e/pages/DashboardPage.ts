import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Dashboard/Overview Page Object
 * Main landing page at /backoffice
 */
export class DashboardPage extends BasePage {
  private readonly portalHeading = 'h2:has-text("Admin Portal")';
  private readonly brandHeading = 'h1:has-text("BookWorm")';
  private readonly navigationMenu = 'nav[aria-label="Main navigation"]';
  private readonly overviewLink = 'a:has-text("Overview")';
  private readonly booksLink = 'a:has-text("Books")';
  private readonly categoriesLink = 'a:has-text("Categories")';
  private readonly authorsLink = 'a:has-text("Authors")';
  private readonly publishersLink = 'a:has-text("Publishers")';
  private readonly ordersLink = 'a:has-text("Orders")';
  private readonly customersLink = 'a:has-text("Customers")';
  private readonly reviewsLink = 'a:has-text("Reviews")';
  private readonly userMenu = 'button[aria-label="User menu"]';
  private readonly logoutMenuItem = '[role="menuitem"]:has-text("Log out")';
  private readonly sidebarLogoutButton =
    'button[aria-label="Logout from admin portal"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to dashboard overview
   */
  async navigate(): Promise<void> {
    await this.goto("/");
  }

  /**
   * Assert dashboard page is loaded
   */
  async assertIsDashboardPage(): Promise<void> {
    await this.assertVisible(this.brandHeading);
    await this.assertVisible(this.portalHeading);
    await this.assertVisible(this.navigationMenu);
    await this.waitForPageLoad();
  }

  /**
   * Navigate to overview section
   */
  async navigateToOverview(): Promise<void> {
    await this.click(this.overviewLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to books section
   */
  async navigateToBooks(): Promise<void> {
    await this.click(this.booksLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to categories section
   */
  async navigateToCategories(): Promise<void> {
    await this.click(this.categoriesLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to authors section
   */
  async navigateToAuthors(): Promise<void> {
    await this.click(this.authorsLink);
    await this.waitForNavigation();
  }

  /**
   * Navigate to publishers section
   */
  async navigateToPublishers(): Promise<void> {
    await this.click(this.publishersLink);
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
   * Open user menu in header
   */
  async openUserMenu(): Promise<void> {
    await this.click(this.userMenu);
  }

  /**
   * Click "Log out" inside the user dropdown menu
   */
  async clickLogoutMenuItem(): Promise<void> {
    await this.click(this.logoutMenuItem);
    await this.waitForNavigation();
  }

  /**
   * Logout via user menu
   */
  async logout(): Promise<void> {
    await this.openUserMenu();
    await this.clickLogoutMenuItem();
  }

  /**
   * Logout via sidebar button
   */
  async logoutFromSidebar(): Promise<void> {
    await this.click(this.sidebarLogoutButton);
    await this.waitForNavigation();
  }

  /**
   * Get count of visible KPI cards on overview page
   */
  async getStatsCount(): Promise<number> {
    const kpiTitles = [
      "Total Revenue",
      "Total Orders",
      "Registered Customers",
      "Books in Catalog",
    ];
    let count = 0;
    for (const title of kpiTitles) {
      if (await this.page.getByText(title).isVisible()) {
        count++;
      }
    }
    return count;
  }

  /**
   * Check if navigation menu is visible
   */
  async isNavigationVisible(): Promise<boolean> {
    return await this.isVisible(this.navigationMenu);
  }
}
