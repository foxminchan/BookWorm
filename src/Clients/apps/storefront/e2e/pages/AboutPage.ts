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
    return this.page.locator('[data-testid="hero-section"], section').first();
  }

  get missionSection(): Locator {
    return this.page.locator(
      '[data-testid="mission-section"], section:has-text("Mission")',
    );
  }

  get valuesSection(): Locator {
    return this.page.locator(
      '[data-testid="values-section"], section:has-text("Values")',
    );
  }

  get timelineSection(): Locator {
    return this.page.locator(
      '[data-testid="timeline-section"], section:has-text("Timeline"), section:has-text("Journey")',
    );
  }

  get contactSection(): Locator {
    return this.page.locator(
      '[data-testid="contact-section"], section:has-text("Contact")',
    );
  }

  get contactForm(): Locator {
    return this.page.locator('form, [data-testid="contact-form"]');
  }

  get nameInput(): Locator {
    return this.page.locator('input[name="name"], input[id*="name"]');
  }

  get emailInput(): Locator {
    return this.page.locator('input[name="email"], input[type="email"]');
  }

  get messageInput(): Locator {
    return this.page.locator(
      'textarea[name="message"], textarea[id*="message"]',
    );
  }

  get submitButton(): Locator {
    return this.page.locator('button[type="submit"], button:has-text("Send")');
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

  async fillContactForm(
    name: string,
    email: string,
    message: string,
  ): Promise<void> {
    await this.nameInput.fill(name);
    await this.emailInput.fill(email);
    await this.messageInput.fill(message);
  }

  async submitContactForm(): Promise<void> {
    await this.submitButton.click();
  }

  // Assertions
  async isOnAboutPage(): Promise<boolean> {
    return this.page.url().includes("/about");
  }
}
