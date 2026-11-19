import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Content pages step definitions (About, Shipping, Returns).
 * Shared steps defined elsewhere:
 *   "I click {string}" → common.steps.ts
 *   "I am on any page" → ai-chatbot.steps.ts
 */

// Footer navigation
When("I scroll to the footer", async ({ page }) => {
  await page.locator("footer").scrollIntoViewIfNeeded();
});

When("I click {string} in the footer", async ({ page }, linkText: string) => {
  const footer = page.locator("footer");
  await footer.scrollIntoViewIfNeeded();
  await footer
    .locator(`a:has-text("${linkText}"), button:has-text("${linkText}")`)
    .first()
    .click();
  await page.waitForLoadState("networkidle");
});

Then("I should see links to {string}", async ({ page }, linkText: string) => {
  await expect(
    page.locator(`footer a:has-text("${linkText}")`).first(),
  ).toBeVisible();
});

Then(
  "I should see footer policy button {string}",
  async ({ page }, buttonText: string) => {
    await expect(
      page.locator(`footer button:has-text("${buttonText}")`).first(),
    ).toBeVisible();
  },
);

// About page
Given("I am on the about page", async ({ aboutPage }) => {
  await aboutPage.navigate();
});

Then("I should be on the about page", ({ aboutPage }) => {
  expect(aboutPage.isOnAboutPage()).toBe(true);
});

When("I scroll through the page", async ({ page }) => {
  await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
  await page.waitForTimeout(300);
});

When("I scroll to the contact section", async ({ aboutPage }) => {
  await aboutPage.scrollToSection("contact");
});

Then("I should see the hero section", async ({ aboutPage }) => {
  await expect(aboutPage.heroSection).toBeVisible();
});

Then("I should see the mission section", async ({ aboutPage }) => {
  await expect(aboutPage.missionSection).toBeVisible();
});

Then("I should see the company mission statement", async ({ aboutPage }) => {
  await expect(aboutPage.missionSection).toBeVisible();
});

Then("I should see the values section", async ({ aboutPage }) => {
  await expect(aboutPage.valuesSection).toBeVisible();
});

Then("I should see the timeline section", async ({ aboutPage }) => {
  await expect(aboutPage.timelineSection).toBeVisible();
});

Then("I should see the company timeline", async ({ aboutPage }) => {
  await expect(aboutPage.timelineSection).toBeVisible();
});

Then("I should see the contact section", async ({ aboutPage }) => {
  await expect(aboutPage.contactSection).toBeVisible();
});

Then("I should see contact information", async ({ aboutPage }) => {
  await expect(aboutPage.contactSection).toBeVisible();
});

Then('I should see the "Get In Touch" mailto link', async ({ aboutPage }) => {
  await expect(aboutPage.contactMailtoLink).toBeVisible();
});

// Shipping page
Given("I am on the shipping page", async ({ shippingPage }) => {
  await shippingPage.navigate();
});

Then("I should be on the shipping page", ({ shippingPage }) => {
  expect(shippingPage.isOnShippingPage()).toBe(true);
});

Then("I should see shipping methods", async ({ shippingPage }) => {
  await expect(shippingPage.shippingMethods).toBeVisible();
});

Then("I should see delivery timeframes", async ({ shippingPage }) => {
  await expect(shippingPage.deliveryTimeframes).toBeVisible();
});

Then("I should see shipping costs information", async ({ shippingPage }) => {
  await expect(shippingPage.shippingCosts).toBeVisible();
});

// Returns page
Given("I am on the returns page", async ({ returnsPage }) => {
  await returnsPage.navigate();
});

Then("I should be on the returns page", ({ returnsPage }) => {
  expect(returnsPage.isOnReturnsPage()).toBe(true);
});

Then("I should see the return policy", async ({ returnsPage }) => {
  await expect(returnsPage.returnPolicy).toBeVisible();
});

Then("I should see the refund process", async ({ returnsPage }) => {
  await expect(returnsPage.refundProcess).toBeVisible();
});

Then("I should see the time window for returns", async ({ returnsPage }) => {
  await expect(returnsPage.returnTimeWindow).toBeVisible();
});

// Policy dialogs
Then("a privacy policy dialog should open", async ({ page }) => {
  const dialog = page.locator('[role="dialog"]');
  await expect(dialog).toBeVisible();
  await expect(dialog).toContainText(/Privacy/i);
});

Then("a terms of service dialog should open", async ({ page }) => {
  const dialog = page.locator('[role="dialog"]');
  await expect(dialog).toBeVisible();
  await expect(dialog).toContainText(/Terms/i);
});

Then("I should be able to close the policy dialog", async ({ page }) => {
  await page.keyboard.press("Escape");
  await expect(page.locator('[role="dialog"]')).not.toBeVisible();
});

// Keyboard & Accessibility
When("I navigate using Tab key", async ({ page }) => {
  await page.keyboard.press("Tab");
  await page.waitForTimeout(200);
});

Then("all interactive elements should be focusable", async ({ page }) => {
  const focused = page.locator(":focus");
  await expect(focused).toBeVisible();
});

Then("section headings should be properly structured", async ({ page }) => {
  const h1Count = await page.locator("h1").count();
  expect(h1Count).toBe(1);
});

// SEO
Then("the page should have a descriptive title", async ({ page }) => {
  const title = await page.title();
  expect(title.length).toBeGreaterThan(0);
});

Then("the page should have meta description", async ({ page }) => {
  const desc = await page
    .locator('meta[name="description"]')
    .getAttribute("content");
  expect(desc).toBeTruthy();
});

Then("the page should have proper heading hierarchy", async ({ page }) => {
  const h1Count = await page.locator("h1").count();
  expect(h1Count).toBe(1);
});

// Mobile
Given("I am on a mobile device", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 812 });
});

When("I visit the about page", async ({ aboutPage }) => {
  await aboutPage.navigate();
});

Then("the content should be readable", async ({ page }) => {
  await expect(page.locator("main")).toBeVisible();
});

Then("navigation should be accessible", async ({ page }) => {
  await expect(
    page.locator('nav[aria-label="Mobile main navigation"]'),
  ).toBeVisible();
});
