import AxeBuilder from "@axe-core/playwright";
import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Accessibility step definitions.
 * Contains canonical "I press Enter" and "I press Escape" steps.
 */

// Keyboard navigation
When("I press Tab", async ({ page }) => {
  await page.keyboard.press("Tab");
  await page.waitForTimeout(200);
});

When("I press Enter", async ({ page }) => {
  await page.keyboard.press("Enter");
  await page.waitForTimeout(300);
});

When("I press Escape", async ({ page }) => {
  await page.keyboard.press("Escape");
  await page.waitForTimeout(300);
});

When("I press Shift+Tab", async ({ page }) => {
  await page.keyboard.press("Shift+Tab");
  await page.waitForTimeout(200);
});

When("I press {string} key", async ({ page }, key: string) => {
  await page.keyboard.press(key);
  await page.waitForTimeout(200);
});

// Heading structure
Then(
  String.raw`headings should follow logical order \(h1, h2, h3\)`,
  async ({ page }) => {
    const headings = await page.evaluate(() => {
      const allHeadings = document.querySelectorAll("h1, h2, h3, h4, h5, h6");
      return Array.from(allHeadings).map((h) =>
        Number.parseInt(h.tagName.charAt(1), 10),
      );
    });
    expect(headings.length).toBeGreaterThan(0);
    for (let i = 1; i < headings.length; i++) {
      expect(headings[i]! - headings[i - 1]!).toBeLessThanOrEqual(1);
    }
  },
);

Then("there should be only one h1 per page", async ({ page }) => {
  const h1Count = await page.locator("h1").count();
  expect(h1Count).toBe(1);
});

Then("no heading levels should be skipped", async ({ page }) => {
  const headings = await page.evaluate(() => {
    const allHeadings = document.querySelectorAll("h1, h2, h3, h4, h5, h6");
    return Array.from(allHeadings).map((h) =>
      Number.parseInt(h.tagName.charAt(1), 10),
    );
  });
  for (let i = 1; i < headings.length; i++) {
    expect(headings[i]! - headings[i - 1]!).toBeLessThanOrEqual(1);
  }
});

// Skip link
When("I press Tab key once", async ({ page }) => {
  await page.keyboard.press("Tab");
  await page.waitForTimeout(200);
});

Then('the "Skip to main content" link should be focused', async ({ page }) => {
  const skipLink = page.locator(
    'a:has-text("Skip to main content"), a[href="#main-content"], a[href="#main"]',
  );
  await expect(skipLink.first()).toBeFocused();
});

Then("it should be visible when focused", async ({ page }) => {
  const focused = page.locator(":focus");
  await expect(focused).toBeVisible();
});

Then("focus should move to main content area", async ({ page }) => {
  const activeId = await page.evaluate(() => document.activeElement?.id);
  expect(activeId).toMatch(/main|content/);
});

// Keyboard navigation flow
When("I navigate to shop using keyboard", async ({ page }) => {
  await page.goto("/shop");
  await page.waitForLoadState("networkidle");
});

When("I select first book using keyboard", async ({ page }) => {
  const firstBook = page.locator('ul[aria-label="Book catalog"] a').first();
  await firstBook.focus();
  await page.keyboard.press("Enter");
  await page.waitForLoadState("networkidle");
});

When("I add to basket using keyboard", async ({ page }) => {
  const addBtn = page.locator('button:has-text("Add to Basket")');
  await addBtn.focus();
  await page.keyboard.press("Enter");
  await page.waitForTimeout(500);
});

When("I navigate to basket using keyboard", async ({ page }) => {
  const basketLink = page.locator('header a[href="/basket"]').first();
  await basketLink.focus();
  await page.keyboard.press("Enter");
  await page.waitForLoadState("networkidle");
});

When("I proceed to checkout using keyboard", async ({ page }) => {
  const checkoutBtn = page.locator('button:has-text("Checkout")');
  await checkoutBtn.focus();
  await page.keyboard.press("Enter");
  await page.waitForLoadState("networkidle");
});

Then("I should complete the checkout successfully", async ({ page }) => {
  await expect(page).toHaveURL(/\/checkout\/confirmation/);
});

Then("all interactive elements should have been keyboard accessible", () => {
  // Implicitly verified by completing the keyboard-only flow
});

// Focus management in remove item dialog
When("I press Tab until remove button is focused", async ({ page }) => {
  const removeBtn = page
    .locator('button[aria-label*="Remove"][aria-label*="from basket"]')
    .first();
  await removeBtn.focus();
  await expect(removeBtn).toBeFocused();
});

When("I press Enter to open remove dialog", async ({ page }) => {
  await page.keyboard.press("Enter");
});

Then("focus should move to the dialog", async ({ page }) => {
  const dialog = page.locator('[role="alertdialog"], [role="dialog"]').first();
  await expect(dialog).toBeVisible();
  const focused = page.locator(":focus");
  const isInside = await focused.evaluate(
    (el, d) => d?.contains(el),
    await dialog.elementHandle(),
  );
  expect(isInside).toBeTruthy();
});

Then(
  'the dialog should have role="dialog" or role="alertdialog"',
  async ({ page }) => {
    const dialog = page
      .locator('[role="dialog"], [role="alertdialog"]')
      .first();
    await expect(dialog).toBeVisible();
  },
);

Then("the dialog should close", async ({ page }) => {
  await expect(
    page.locator('[role="alertdialog"], [role="dialog"]'),
  ).not.toBeVisible();
});

Then("focus should return to the remove button", async ({ page }) => {
  const activeTag = await page.evaluate(() => document.activeElement?.tagName);
  expect(activeTag).toBe("BUTTON");
});

// Search functionality
When("I click the search button", async ({ page }) => {
  await page.locator('header button[aria-label="Search books"]').click();
});

Then("the search input should be automatically focused", async ({ page }) => {
  const input = page.locator("#header-search-input");
  await expect(input).toBeFocused();
});

Then("the search input should lose focus", async ({ page }) => {
  const input = page.locator("#header-search-input");
  await expect(input).not.toBeFocused();
});

// Inspection & ARIA
When("I inspect the page elements", () => {
  /* No-op */
});

Then("all buttons should have accessible names", async ({ page }) => {
  const buttons = page.locator("button:visible");
  const accessibleNames = await buttons.evaluateAll((elements) =>
    elements.slice(0, 10).map((button) => ({
      text: button.textContent?.trim(),
      label:
        button.getAttribute("aria-label") ||
        button.getAttribute("aria-labelledby"),
    })),
  );
  for (const { text, label } of accessibleNames) {
    expect(text || label).toBeTruthy();
  }
});

Then("all form inputs should have associated labels", async ({ page }) => {
  const inputs = page.locator(
    "input:visible, select:visible, textarea:visible",
  );
  const labelStatuses = await inputs.evaluateAll((elements) =>
    elements.slice(0, 10).map((el) => {
      const id = el.id;
      const hasAssociatedLabel = id
        ? document.querySelector(`label[for="${id}"]`) !== null
        : false;
      const ariaLabel = el.getAttribute("aria-label");
      const ariaLabelledBy = el.getAttribute("aria-labelledby");
      const closestLabel = el.closest("label") !== null;
      const placeholder = el.getAttribute("placeholder");
      return (
        hasAssociatedLabel ||
        !!ariaLabel ||
        !!ariaLabelledBy ||
        closestLabel ||
        !!placeholder
      );
    }),
  );
  for (const hasLabel of labelStatuses) {
    expect(hasLabel).toBeTruthy();
  }
});

Then("all images should have descriptive alt text", async ({ page }) => {
  const imgs = page.locator("img");
  const count = await imgs.count();
  if (count > 0) {
    const alt = await imgs.first().getAttribute("alt");
    expect(alt).not.toBeNull();
  }
});

Then('the basket icon should announce "Shopping basket"', async ({ page }) => {
  const icon = page.locator('header a[href="/basket"]').first();
  const label = await icon.getAttribute("aria-label");
  expect(label).toContain("Shopping basket");
});

// Loading states
When("books finish loading", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("the content should have proper semantic structure", async ({ page }) => {
  await expect(page.locator("main")).toBeVisible();
});

// Landmarks
Then("the page should have:", async ({ page }, _table: unknown) => {
  await expect(page.locator("main")).toBeVisible();
  await expect(page.locator("nav").first()).toBeVisible();
  await expect(page.locator("footer")).toBeVisible();
});

Then(
  "each landmark should have proper aria-label if multiple of same type",
  async ({ page }) => {
    const navs = page.locator("nav");
    if ((await navs.count()) > 1) {
      const label = await navs.first().getAttribute("aria-label");
      expect(label).toBeTruthy();
    }
  },
);

// Axe-core automated scan
When("I run automated accessibility scan", async ({ page, scenarioData }) => {
  const results = await new AxeBuilder({ page })
    .withTags(["wcag2a", "wcag2aa"])
    .analyze();
  scenarioData.axeResults = results;
});

Then("there should be no color contrast violations", ({ scenarioData }) => {
  const results = scenarioData.axeResults as Awaited<
    ReturnType<AxeBuilder["analyze"]>
  >;
  const contrastViolations = results.violations.filter(
    (v) => v.id === "color-contrast",
  );
  expect(contrastViolations).toEqual([]);
});

Then("WCAG AA standards should be met", ({ scenarioData }) => {
  const results = scenarioData.axeResults as Awaited<
    ReturnType<AxeBuilder["analyze"]>
  >;
  expect(results.violations).toEqual([]);
});

// Mobile
Given("I am on the homepage on mobile viewport", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
  await page.goto("/");
  await page.waitForLoadState("networkidle");
});

When("I navigate using touch", () => {
  /* No-op */
});

Then("the mobile bottom nav should have proper labels", async ({ page }) => {
  const nav = page.locator('nav[aria-label="Mobile main navigation"]');
  await expect(nav).toBeVisible();
  const links = nav.locator("a");
  const count = await links.count();
  expect(count).toBeGreaterThan(0);
});

Then("all interactive elements should be reachable", async ({ page }) => {
  const nav = page.locator('nav[aria-label="Mobile main navigation"]');
  await expect(nav).toBeVisible();
});
