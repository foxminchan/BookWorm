import { Locator, Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * About Page Object Model
 * Represents the about page at '/about'
 */
export class AboutPage extends BasePage {
  readonly path = "/about";

  constructor(page: Page) {
    super(page);
  }

  // Locators
  get heroSection(): Locator {
    return this.page.locator("main section").first();
  }

  get missionSection(): Locator {
    return this.page.locator('main section:has-text("Mission")').first();
  }

  get valuesSection(): Locator {
    return this.page.locator('main section:has-text("Values")').first();
  }

  get timelineSection(): Locator {
    return this.page
      .locator(
        'main section:has-text("Timeline"), main section:has-text("Journey")',
      )
      .first();
  }

  get contactSection(): Locator {
    return this.page.locator('main section:has-text("Get In Touch")').first();
  }

  get contactMailtoLink(): Locator {
    return this.page.locator('a[href="mailto:support@bookworm.com"]');
  }

  // Actions
  async navigate(): Promise<void> {
    await this.goto(this.path);
  }

  async scrollToSection(
    section: "mission" | "values" | "timeline" | "contact",
  ): Promise<void> {
    const sectionMap = {
      mission: this.missionSection,
      values: this.valuesSection,
      timeline: this.timelineSection,
      contact: this.contactSection,
    };

    await sectionMap[section].scrollIntoViewIfNeeded();
  }

  // Assertions
  isOnAboutPage(): boolean {
    return this.page.url().includes("/about");
  }
}
