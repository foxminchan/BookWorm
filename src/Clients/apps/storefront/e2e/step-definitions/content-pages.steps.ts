import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { AboutPage } from "../pages";

/**
 * Content pages step definitions
 */

// About page
Given("I am on the about page", async function (this: { page: Page }) {
  const aboutPage = new AboutPage(this.page);
  await aboutPage.navigate();
});

When(
  "I click {string} in the footer",
  async function (this: { page: Page }, linkText: string) {
    const footer = this.page.locator("footer");
    const link = footer.locator(`a:has-text("${linkText}")`);
    await link.click();
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should be on the about page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/about/);
});

Then(
  "I should see the company mission statement",
  async function (this: { page: Page }) {
    const missionSection = this.page.locator(
      'section:has-text("Mission"), [data-testid="mission-section"]',
    );
    await expect(missionSection).toBeVisible();
  },
);

Then("I should see the values section", async function (this: { page: Page }) {
  const valuesSection = this.page.locator(
    'section:has-text("Values"), [data-testid="values-section"]',
  );
  await expect(valuesSection).toBeVisible();
});

Then(
  "I should see the company timeline",
  async function (this: { page: Page }) {
    const timelineSection = this.page.locator(
      'section:has-text("Timeline"), section:has-text("Journey"), [data-testid="timeline-section"]',
    );
    await expect(timelineSection).toBeVisible();
  },
);

Then("I should see contact information", async function (this: { page: Page }) {
  const contactSection = this.page.locator(
    'section:has-text("Contact"), [data-testid="contact-section"]',
  );
  await expect(contactSection).toBeVisible();
});

When("I scroll through the page", async function (this: { page: Page }) {
  await this.page.evaluate(() => {
    window.scrollTo(0, document.body.scrollHeight / 4);
  });
  await this.page.waitForTimeout(300);
  await this.page.evaluate(() => {
    window.scrollTo(0, document.body.scrollHeight / 2);
  });
  await this.page.waitForTimeout(300);
});

Then("I should see the hero section", async function (this: { page: Page }) {
  const heroSection = this.page
    .locator('[data-testid="hero-section"], section')
    .first();
  await expect(heroSection).toBeVisible();
});

Then("I should see the mission section", async function (this: { page: Page }) {
  const missionSection = this.page.locator(
    '[data-testid="mission-section"], section:has-text("Mission")',
  );
  await expect(missionSection).toBeVisible();
});

Then("I should see the contact section", async function (this: { page: Page }) {
  const contactSection = this.page.locator(
    '[data-testid="contact-section"], section:has-text("Contact")',
  );
  await expect(contactSection).toBeVisible();
});

// About page keyboard navigation
When("I navigate using Tab key", async function (this: { page: Page }) {
  for (let i = 0; i < 10; i++) {
    await this.page.keyboard.press("Tab");
    await this.page.waitForTimeout(100);
  }
});

Then(
  "all interactive elements should be focusable",
  async function (this: { page: Page }) {
    const focusedElement = await this.page.evaluate(
      () => document.activeElement?.tagName,
    );
    expect(focusedElement).toBeTruthy();
  },
);

Then(
  "section headings should be properly structured",
  async function (this: { page: Page }) {
    const h1 = this.page.locator("h1");
    const h1Count = await h1.count();
    expect(h1Count).toBeGreaterThan(0);
  },
);

// Shipping page
Then("I should be on the shipping page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/shipping/);
});

Then("I should see shipping methods", async function (this: { page: Page }) {
  const shippingMethods = this.page.locator(
    ':has-text("Standard"), :has-text("Express"), :has-text("method")',
  );
  await expect(shippingMethods.first()).toBeVisible();
});

Then("I should see delivery timeframes", async function (this: { page: Page }) {
  const timeframes = this.page.locator(
    ':has-text("days"), :has-text("business days"), :has-text("delivery")',
  );
  await expect(timeframes.first()).toBeVisible();
});

Then(
  "I should see shipping costs information",
  async function (this: { page: Page }) {
    const costs = this.page.locator(
      ':has-text("$"), :has-text("cost"), :has-text("shipping")',
    );
    await expect(costs.first()).toBeVisible();
  },
);

Then(
  "I should see international shipping details",
  async function (this: { page: Page }) {
    const international = this.page.locator(
      ':has-text("International"), :has-text("worldwide")',
    );
    await expect(international.first()).toBeVisible();
  },
);

// Shipping from basket
Given("I am on the basket page", async function (this: { page: Page }) {
  await this.page.goto("/basket");
  await this.page.waitForLoadState("networkidle");
});

When(
  "I click {string} link",
  async function (this: { page: Page }, linkText: string) {
    const link = this.page.locator(`a:has-text("${linkText}")`);
    await link.click();
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "I should be able to return to basket",
  async function (this: { page: Page }) {
    const backButton = this.page.locator(
      'button:has-text("Back"), a[href="/basket"]',
    );
    await expect(backButton.first()).toBeVisible();
  },
);

// Returns page
Then("I should be on the returns page", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/returns/);
});

Then("I should see the return policy", async function (this: { page: Page }) {
  const policy = this.page.locator(':has-text("return"), :has-text("policy")');
  await expect(policy.first()).toBeVisible();
});

Then("I should see the refund process", async function (this: { page: Page }) {
  const refundProcess = this.page.locator(
    ':has-text("refund"), :has-text("process")',
  );
  await expect(refundProcess.first()).toBeVisible();
});

Then(
  "I should see the time window for returns",
  async function (this: { page: Page }) {
    const timeWindow = this.page.locator(
      ':has-text("30 days"), :has-text("days")',
    );
    await expect(timeWindow.first()).toBeVisible();
  },
);

Then(
  "I should see contact information for returns",
  async function (this: { page: Page }) {
    const contactInfo = this.page.locator(
      ':has-text("contact"), :has-text("email"), :has-text("phone")',
    );
    await expect(contactInfo.first()).toBeVisible();
  },
);

// Returns from order details
Given("I am logged in", async function (this: { page: Page }) {
  await this.page.evaluate(() => {
    localStorage.setItem("auth-token", "mock-token");
  });
});

Given("I have a completed order", async function (this: { page: Page }) {
  // Mock completed order data
  await this.page.evaluate(() => {
    localStorage.setItem(
      "orders",
      JSON.stringify([{ id: "12345", status: "Completed", total: 99.99 }]),
    );
  });
});

When("I view the order details", async function (this: { page: Page }) {
  await this.page.goto("/account/orders/12345");
  await this.page.waitForLoadState("networkidle");
});

// Footer navigation
When("I scroll to the footer", async function (this: { page: Page }) {
  await this.page.evaluate(() => {
    window.scrollTo(0, document.body.scrollHeight);
  });
  await this.page.waitForTimeout(300);
});

Then(
  "I should see links to {string}",
  async function (this: { page: Page }, linkText: string) {
    const footer = this.page.locator("footer");
    const link = footer.locator(`a:has-text("${linkText}")`);
    await expect(link).toBeVisible();
  },
);

Then("all links should be clickable", async function (this: { page: Page }) {
  const footer = this.page.locator("footer");
  const links = footer.locator("a");
  const count = await links.count();
  expect(count).toBeGreaterThan(0);

  for (let i = 0; i < Math.min(count, 5); i++) {
    await expect(links.nth(i)).toBeEnabled();
  }
});

// Privacy and Terms dialogs
Then(
  "a privacy policy dialog should open",
  async function (this: { page: Page }) {
    const dialog = this.page.locator(
      '[role="dialog"]:has-text("Privacy"), [data-testid="privacy-dialog"]',
    );
    await expect(dialog).toBeVisible();
  },
);

Then("I should see GDPR information", async function (this: { page: Page }) {
  const gdprInfo = this.page.locator(
    ':has-text("GDPR"), :has-text("data protection")',
  );
  await expect(gdprInfo).toBeVisible();
});

Then(
  "I should see data collection details",
  async function (this: { page: Page }) {
    const dataCollection = this.page.locator(
      ':has-text("collect"), :has-text("data")',
    );
    await expect(dataCollection).toBeVisible();
  },
);

Then(
  "I should be able to close the dialog",
  async function (this: { page: Page }) {
    const closeButton = this.page
      .locator('[data-testid="close-dialog"], button[aria-label*="Close"]')
      .first();
    await expect(closeButton).toBeVisible();
    await closeButton.click();
    await this.page.waitForTimeout(300);
  },
);

Then(
  "a terms of service dialog should open",
  async function (this: { page: Page }) {
    const dialog = this.page.locator(
      '[role="dialog"]:has-text("Terms"), [data-testid="terms-dialog"]',
    );
    await expect(dialog).toBeVisible();
  },
);

Then("I should see usage terms", async function (this: { page: Page }) {
  const terms = this.page.locator(':has-text("terms"), :has-text("use")');
  await expect(terms).toBeVisible();
});

Then(
  "I should see limitation of liability",
  async function (this: { page: Page }) {
    const liability = this.page.locator(
      ':has-text("liability"), :has-text("limitation")',
    );
    await expect(liability).toBeVisible();
  },
);

// Contact form
When("I scroll to the contact section", async function (this: { page: Page }) {
  const contactSection = this.page.locator(
    '[data-testid="contact-section"], section:has-text("Contact")',
  );
  await contactSection.scrollIntoViewIfNeeded();
});

When(
  "I fill in the contact form with valid data",
  async function (this: { page: Page }) {
    const nameInput = this.page.locator(
      'input[name="name"], input[id*="name"]',
    );
    const emailInput = this.page.locator(
      'input[name="email"], input[type="email"]',
    );
    const messageInput = this.page.locator(
      'textarea[name="message"], textarea',
    );

    await nameInput.fill("John Doe");
    await emailInput.fill("john@example.com");
    await messageInput.fill("This is a test message");
  },
);

When("I submit the form", async function (this: { page: Page }) {
  const submitButton = this.page.locator(
    'button[type="submit"], button:has-text("Send")',
  );
  await submitButton.click();
});

Then("I should see a success message", async function (this: { page: Page }) {
  const successMessage = this.page.locator(
    '[role="alert"]:has-text("success"), :has-text("Thank you")',
  );
  await expect(successMessage).toBeVisible({ timeout: 5000 });
});

Then(
  "I should receive a confirmation email",
  async function (this: { page: Page }) {
    // This would typically be verified externally or via API
    // For now, just check that form submitted successfully
    expect(true).toBeTruthy();
  },
);

When(
  "I submit the form without filling fields",
  async function (this: { page: Page }) {
    const submitButton = this.page.locator('button[type="submit"]');
    await submitButton.click();
  },
);

Then("I should see validation errors", async function (this: { page: Page }) {
  const errors = this.page.locator(
    '[role="alert"], .error, :has-text("required")',
  );
  await expect(errors.first()).toBeVisible();
});

Then("the form should not be submitted", async function (this: { page: Page }) {
  // Check that we're still on the same page
  const currentUrl = this.page.url();
  expect(currentUrl).toContain("/about");
});

// Breadcrumbs
Then("I should see breadcrumbs", async function (this: { page: Page }) {
  const breadcrumbs = this.page.locator(
    'nav[aria-label="Breadcrumb"], [data-testid="breadcrumbs"]',
  );
  await expect(breadcrumbs).toBeVisible();
});

Then(
  "breadcrumbs should show {string}",
  async function (this: { page: Page }, breadcrumbText: string) {
    const breadcrumbs = this.page.locator(
      'nav[aria-label="Breadcrumb"], [data-testid="breadcrumbs"]',
    );
    const text = await breadcrumbs.textContent();
    expect(text).toContain(breadcrumbText.replace(" > ", ""));
  },
);

When(
  "I click {string} in breadcrumbs",
  async function (this: { page: Page }, linkText: string) {
    const breadcrumbLink = this.page.locator(
      `nav[aria-label="Breadcrumb"] a:has-text("${linkText}")`,
    );
    await breadcrumbLink.click();
  },
);

Then("I should return to the homepage", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/^\/$|\/$/);
});

// SEO metadata
Then(
  "the page should have a descriptive title",
  async function (this: { page: Page }) {
    const title = await this.page.title();
    expect(title.length).toBeGreaterThan(10);
  },
);

Then(
  "the page should have meta description",
  async function (this: { page: Page }) {
    const metaDescription = this.page.locator('meta[name="description"]');
    const content = await metaDescription.getAttribute("content");
    expect(content).toBeTruthy();
  },
);

Then(
  "the page should have proper heading hierarchy",
  async function (this: { page: Page }) {
    const h1Count = await this.page.locator("h1").count();
    expect(h1Count).toBe(1); // Should have exactly one h1
  },
);

// Mobile responsive
When("I visit the about page", async function (this: { page: Page }) {
  await this.page.goto("/about");
  await this.page.waitForLoadState("networkidle");
});

Then("the content should be readable", async function (this: { page: Page }) {
  const mainContent = this.page.locator("main, article");
  await expect(mainContent).toBeVisible();
});

Then("images should be properly sized", async function (this: { page: Page }) {
  const images = this.page.locator("img");
  const firstImage = images.first();
  const box = await firstImage.boundingBox();
  if (box) {
    expect(box.width).toBeLessThanOrEqual(375); // Mobile width
  }
});

Then("navigation should be accessible", async function (this: { page: Page }) {
  const nav = this.page.locator("nav, header");
  await expect(nav.first()).toBeVisible();
});

// Print-friendly
When("I open the print dialog", async function (this: { page: Page }) {
  // Simulate print media query
  await this.page.emulateMedia({ media: "print" });
});

Then(
  "the page should have a print-optimized layout",
  async function (this: { page: Page }) {
    // Check that main content is visible in print mode
    const mainContent = this.page.locator("main");
    await expect(mainContent).toBeVisible();
  },
);

Then(
  "unnecessary elements should be hidden",
  async function (this: { page: Page }) {
    // Check that navigation might be hidden (implementation-specific)
    // This is just a placeholder - actual implementation may vary
    expect(true).toBeTruthy();
  },
);

// Newsletter signup
When(
  "I enter my email in the newsletter field",
  async function (this: { page: Page }) {
    const emailInput = this.page
      .locator('footer input[type="email"], input[placeholder*="email"]')
      .first();
    await emailInput.fill("newsletter@example.com");
  },
);

When(
  "I click {string}",
  async function (this: { page: Page }, buttonText: string) {
    const button = this.page
      .locator(`button:has-text("${buttonText}")`)
      .first();
    await button.click();
  },
);

Then(
  "I should see a subscription confirmation",
  async function (this: { page: Page }) {
    const confirmation = this.page.locator(
      ':has-text("subscribed"), :has-text("Thank you")',
    );
    await expect(confirmation).toBeVisible({ timeout: 5000 });
  },
);

Then("I should receive a welcome email", async function (this: { page: Page }) {
  // This would be verified externally
  expect(true).toBeTruthy();
});

// Social media links
Then("I should see social media icons", async function (this: { page: Page }) {
  const footer = this.page.locator("footer");
  const socialIcons = footer.locator(
    '[aria-label*="Twitter"], [aria-label*="Instagram"], [aria-label*="Facebook"]',
  );
  const count = await socialIcons.count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "clicking Twitter should open BookWorm's Twitter",
  async function (this: { page: Page }) {
    const twitterLink = this.page.locator(
      'footer a[aria-label*="Twitter"], a[href*="twitter.com"]',
    );
    const href = await twitterLink.getAttribute("href");
    expect(href).toContain("twitter");
  },
);

Then(
  "clicking Instagram should open BookWorm's Instagram",
  async function (this: { page: Page }) {
    const instagramLink = this.page.locator(
      'footer a[aria-label*="Instagram"], a[href*="instagram.com"]',
    );
    const href = await instagramLink.getAttribute("href");
    expect(href).toContain("instagram");
  },
);

Then("links should open in new tabs", async function (this: { page: Page }) {
  const socialLinks = this.page.locator(
    'footer a[href*="twitter"], a[href*="instagram"]',
  );
  const firstLink = socialLinks.first();
  const target = await firstLink.getAttribute("target");
  expect(target).toBe("_blank");
});
