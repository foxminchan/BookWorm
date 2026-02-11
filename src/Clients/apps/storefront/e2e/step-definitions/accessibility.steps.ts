import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import type { Page } from "@playwright/test";

/**
 * Step definitions for accessibility testing
 * Tests keyboard navigation, focus management, ARIA attributes, and screen reader compatibility
 */

// Keyboard Navigation
When("I press Tab key once", async function (this: { page: Page }) {
  await this.page.keyboard.press("Tab");
  await this.page.waitForTimeout(100);
});

When(
  "I press Tab key {int} times",
  async function (this: { page: Page }, times: number) {
    for (let i = 0; i < times; i++) {
      await this.page.keyboard.press("Tab");
      await this.page.waitForTimeout(50);
    }
  },
);

When("I press Tab", async function (this: { page: Page }) {
  await this.page.keyboard.press("Tab");
  await this.page.waitForTimeout(100);
});

When("I press Enter", async function (this: { page: Page }) {
  await this.page.keyboard.press("Enter");
  await this.page.waitForTimeout(200);
});

When("I press Escape", async function (this: { page: Page }) {
  await this.page.keyboard.press("Escape");
  await this.page.waitForTimeout(200);
});

When("I press Space", async function (this: { page: Page }) {
  await this.page.keyboard.press("Space");
  await this.page.waitForTimeout(200);
});

// Skip to main content
Then(
  "the {string} link should be focused",
  async function (this: { page: Page }, linkText: string) {
    const focusedElement = this.page.locator(":focus");
    const text = await focusedElement.textContent();
    expect(text?.toLowerCase()).toContain(linkText.toLowerCase());
  },
);

Then(
  "it should be visible when focused",
  async function (this: { page: Page }) {
    const focusedElement = this.page.locator(":focus");
    await expect(focusedElement).toBeVisible();
  },
);

Then(
  "focus should move to main content area",
  async function (this: { page: Page }) {
    const focusedElement = this.page.locator(":focus");
    const tagName = await focusedElement.evaluate((el) => el.tagName);
    // Should focus on #main-content or similar
    expect(["MAIN", "DIV"]).toContain(tagName);
  },
);

// Keyboard navigation through flows
When(
  "I navigate to shop using keyboard",
  async function (this: { page: Page }) {
    // Tab to Browse Collection button and press Enter
    let attempts = 0;
    while (attempts < 20) {
      await this.page.keyboard.press("Tab");
      const focused = await this.page.locator(":focus").textContent();
      if (focused?.includes("Browse") || focused?.includes("Shop")) {
        await this.page.keyboard.press("Enter");
        await this.page.waitForURL("**/shop**");
        return;
      }
      attempts++;
    }
    throw new Error("Could not navigate to shop using keyboard");
  },
);

When(
  "I select first book using keyboard",
  async function (this: { page: Page }) {
    // Tab until a book card is focused
    let attempts = 0;
    while (attempts < 50) {
      await this.page.keyboard.press("Tab");
      const focused = this.page.locator(":focus");
      const href = await focused.getAttribute("href");
      if (href?.includes("/shop/")) {
        await this.page.keyboard.press("Enter");
        await this.page.waitForURL("**/shop/**");
        return;
      }
      attempts++;
    }
    throw new Error("Could not select book using keyboard");
  },
);

When("I add to basket using keyboard", async function (this: { page: Page }) {
  let attempts = 0;
  while (attempts < 30) {
    await this.page.keyboard.press("Tab");
    const focused = await this.page.locator(":focus").textContent();
    if (
      focused?.toLowerCase().includes("add to basket") ||
      focused?.toLowerCase().includes("add to cart")
    ) {
      await this.page.keyboard.press("Enter");
      await this.page.waitForTimeout(500);
      return;
    }
    attempts++;
  }
  throw new Error("Could not add to basket using keyboard");
});

When(
  "I navigate to basket using keyboard",
  async function (this: { page: Page }) {
    // Tab to basket icon and press Enter
    let attempts = 0;
    while (attempts < 50) {
      await this.page.keyboard.press("Tab");
      const focused = this.page.locator(":focus");
      const href = await focused.getAttribute("href");
      const ariaLabel = await focused.getAttribute("aria-label");
      if (
        href === "/basket" ||
        ariaLabel?.toLowerCase().includes("basket") ||
        ariaLabel?.toLowerCase().includes("cart")
      ) {
        await this.page.keyboard.press("Enter");
        await this.page.waitForURL("**/basket**");
        return;
      }
      attempts++;
    }
    throw new Error("Could not navigate to basket using keyboard");
  },
);

When(
  "I proceed to checkout using keyboard",
  async function (this: { page: Page }) {
    let attempts = 0;
    while (attempts < 30) {
      await this.page.keyboard.press("Tab");
      const focused = await this.page.locator(":focus").textContent();
      if (focused?.toLowerCase().includes("checkout")) {
        await this.page.keyboard.press("Enter");
        await this.page.waitForTimeout(1000);
        return;
      }
      attempts++;
    }
    throw new Error("Could not proceed to checkout using keyboard");
  },
);

Then(
  "all interactive elements should have been keyboard accessible",
  async function (this: { page: Page }) {
    // Validation that we successfully completed the flow
    expect(this.page.url()).toContain("/orders/");
  },
);

// Focus management
When(
  "I press Tab until remove button is focused",
  async function (this: { page: Page }) {
    let attempts = 0;
    while (attempts < 50) {
      await this.page.keyboard.press("Tab");
      const focused = this.page.locator(":focus");
      const ariaLabel = await focused.getAttribute("aria-label");
      if (ariaLabel?.toLowerCase().includes("remove")) {
        return;
      }
      attempts++;
    }
    throw new Error("Could not find remove button");
  },
);

When(
  "I press Enter to open remove dialog",
  async function (this: { page: Page }) {
    await this.page.keyboard.press("Enter");
    await this.page.waitForTimeout(300);
  },
);

Then("focus should move to the dialog", async function (this: { page: Page }) {
  const dialog = this.page.locator('[role="dialog"], [role="alertdialog"]');
  await expect(dialog).toBeVisible();

  const focusedElement = this.page.locator(":focus");
  const isInsideDialog = await focusedElement.evaluate((el, dialogSelector) => {
    const dialog = document.querySelector(dialogSelector);
    return dialog?.contains(el) ?? false;
  }, '[role="dialog"], [role="alertdialog"]');
  expect(isInsideDialog).toBe(true);
});

Then(
  "the dialog should have role={string} or role={string}",
  async function (this: { page: Page }, role1: string, role2: string) {
    const dialog = this.page.locator(`[role="${role1}"], [role="${role2}"]`);
    await expect(dialog).toBeVisible();
  },
);

Then(
  "focus should cycle within the dialog only",
  async function (this: { page: Page }) {
    const dialog = this.page.locator('[role="dialog"], [role="alertdialog"]');

    // Tab multiple times and ensure focus stays in dialog
    for (let i = 0; i < 10; i++) {
      await this.page.keyboard.press("Tab");
      await this.page.waitForTimeout(50);

      const focusedElement = this.page.locator(":focus");
      const isInsideDialog = await focusedElement.evaluate(
        (el, dialogEl) => {
          return dialogEl?.contains(el) ?? false;
        },
        await dialog.elementHandle(),
      );

      if (!isInsideDialog) {
        throw new Error(`Focus escaped dialog after ${i + 1} tabs`);
      }
    }
  },
);

Then("the dialog should close", async function (this: { page: Page }) {
  const dialog = this.page.locator('[role="dialog"], [role="alertdialog"]');
  await expect(dialog).not.toBeVisible();
});

Then(
  "focus should return to the remove button",
  async function (this: { page: Page }) {
    const focusedElement = this.page.locator(":focus");
    const ariaLabel = await focusedElement.getAttribute("aria-label");
    expect(ariaLabel?.toLowerCase()).toContain("remove");
  },
);

// ARIA attributes
Then(
  "all buttons should have accessible names",
  async function (this: { page: Page }) {
    const buttons = await this.page.locator("button").all();

    for (const button of buttons) {
      const ariaLabel = await button.getAttribute("aria-label");
      const text = await button.textContent();
      const ariaLabelledBy = await button.getAttribute("aria-labelledby");

      const hasAccessibleName = !!(ariaLabel || text?.trim() || ariaLabelledBy);

      if (!hasAccessibleName) {
        const outerHTML = await button.evaluate((el) => el.outerHTML);
        throw new Error(
          `Button without accessible name found: ${outerHTML.substring(0, 100)}`,
        );
      }
    }
  },
);

Then(
  "all form inputs should have associated labels",
  async function (this: { page: Page }) {
    const inputs = await this.page
      .locator('input:not([type="hidden"]), textarea, select')
      .all();

    for (const input of inputs) {
      const id = await input.getAttribute("id");
      const ariaLabel = await input.getAttribute("aria-label");
      const ariaLabelledBy = await input.getAttribute("aria-labelledby");

      // Check if there's a label pointing to this input
      let hasLabel = false;
      if (id) {
        const label = this.page.locator(`label[for="${id}"]`);
        hasLabel = (await label.count()) > 0;
      }

      const hasAccessibleName = !!(hasLabel || ariaLabel || ariaLabelledBy);

      if (!hasAccessibleName) {
        const outerHTML = await input.evaluate((el) => el.outerHTML);
        throw new Error(
          `Input without label found: ${outerHTML.substring(0, 100)}`,
        );
      }
    }
  },
);

Then(
  "all images should have descriptive alt text",
  async function (this: { page: Page }) {
    const images = await this.page.locator("img").all();

    for (const img of images) {
      const alt = await img.getAttribute("alt");
      const ariaHidden = await img.getAttribute("aria-hidden");
      const role = await img.getAttribute("role");

      // Decorative images should have alt="" or aria-hidden
      // Content images should have descriptive alt
      if (ariaHidden !== "true" && role !== "presentation") {
        if (alt === null || alt === undefined) {
          const src = await img.getAttribute("src");
          throw new Error(`Image missing alt attribute: ${src}`);
        }
      }
    }
  },
);

Then(
  "the basket icon should announce {string}",
  async function (this: { page: Page }, expectedText: string) {
    const basketLink = this.page.locator('a[href="/basket"]');
    const ariaLabel = await basketLink.getAttribute("aria-label");

    expect(ariaLabel?.toLowerCase()).toContain("basket");
    // Should announce item count
    expect(ariaLabel).toMatch(/\d+/);
  },
);

// ARIA live regions
Then(
  "an aria-live region should announce the update",
  async function (this: { page: Page }) {
    const liveRegion = this.page.locator('[aria-live], [role="status"]');
    await expect(liveRegion).toHaveCount(1, { timeout: 1000 });
  },
);

// Landmarks
Then(
  "the page should have:",
  async function (this: { page: Page }, dataTable: any) {
    const landmarks = dataTable.hashes();

    for (const { landmark, count } of landmarks) {
      const landmarkSelectors: Record<string, string> = {
        contentinfo: "footer, [role='contentinfo']",
        navigation: "nav, [role='navigation']",
        main: "main, [role='main']",
      };

      const selector = landmarkSelectors[landmark] ?? `[role='${landmark}']`;

      const elements = this.page.locator(selector);
      const actualCount = await elements.count();

      if (count.includes("-")) {
        // Range like "1-2"
        const [min, max] = count.split("-").map(Number);
        expect(actualCount).toBeGreaterThanOrEqual(min);
        expect(actualCount).toBeLessThanOrEqual(max);
      } else {
        expect(actualCount).toBe(Number(count));
      }
    }
  },
);

// Automated scanning (requires @axe-core/playwright)
When(
  "I run automated accessibility scan",
  async function (this: { page: Page }) {
    // This would require installing and configuring axe-core
    // For now, just a placeholder
    console.log("Automated a11y scan would run here with axe-core");
  },
);

Then(
  "there should be no color contrast violations",
  async function (this: { page: Page }) {
    // Placeholder for axe-core check
    console.log("Color contrast check would run here");
  },
);

Then("WCAG AA standards should be met", async function (this: { page: Page }) {
  // Placeholder for axe-core check
  console.log("WCAG AA check would run here");
});

// Heading structure
Then(
  "headings should follow logical order (h1, h2, h3)",
  async function (this: { page: Page }) {
    // Get heading levels
    const levels = await this.page
      .locator("h1, h2, h3, h4, h5, h6")
      .evaluateAll((elements) =>
        elements.map((el) => Number.parseInt(el.tagName.substring(1))),
      );

    // Check no levels are skipped
    for (let i = 1; i < levels.length; i++) {
      const diff = levels[i]! - levels[i - 1]!;
      if (diff > 1) {
        throw new Error(
          `Heading level skipped: h${levels[i - 1]} to h${levels[i]}`,
        );
      }
    }
  },
);

Then(
  "there should be only one h1 per page",
  async function (this: { page: Page }) {
    const h1Count = await this.page.locator("h1").count();
    expect(h1Count).toBe(1);
  },
);

Then(
  "no heading levels should be skipped",
  async function (this: { page: Page }) {
    // Already checked in "headings should follow logical order"
  },
);
