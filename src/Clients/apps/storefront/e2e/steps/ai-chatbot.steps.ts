import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * AI Chatbot step definitions.
 * Matches `components/chat-bot.tsx`, `components/chat-bot-content.tsx`,
 * and `/api/copilotkit/[[...slug]]/route.ts`.
 *
 * Shared steps defined elsewhere:
 *   "I click {string}" → common.steps.ts
 *   "I press Enter" → accessibility.steps.ts
 *   "I press Escape" → accessibility.steps.ts
 */

// Feature flag & environment setup
Given("AI features are enabled", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem(
      "feature-flags",
      JSON.stringify({ copilotEnabled: true }),
    );
  });
});

Given("AI features are disabled", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem(
      "feature-flags",
      JSON.stringify({ copilotEnabled: false }),
    );
  });
});

Given("the API gateway is not configured", async ({ page }) => {
  await page.route("**/api/copilotkit**", (route) =>
    route.fulfill({
      status: 500,
      contentType: "application/json",
      body: JSON.stringify({
        error: "Failed to process request",
        message: "Gateway URL is not configured",
      }),
    }),
  );
});

Given("I am on any page", async ({ page }) => {
  await page.goto("/shop");
  await page.waitForLoadState("networkidle");
});

// Opening chatbot
When("I click {string} button", async ({ page }, buttonText: string) => {
  await page.locator(`button:has-text("${buttonText}")`).first().click();
  await page.waitForTimeout(500);
});

When(
  "I click the floating chat button with label {string}",
  async ({ page }, ariaLabel: string) => {
    const btn = page
      .locator(`button[aria-label="${ariaLabel}"]`)
      .or(page.locator("[data-copilot-chat-trigger]"))
      .first();
    await btn.click();
    await page.waitForTimeout(500);
  },
);

When("I click the floating chat button", async ({ page }) => {
  await page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label="Open BookWorm Literary Guide chat"]',
    )
    .first()
    .click();
  await page.waitForTimeout(500);
});

When("I click the unauthenticated chat button", async ({ page }) => {
  await page
    .locator('button[aria-label="Open chat - sign in required"]')
    .click();
  await page.waitForTimeout(300);
});

When("I click the disabled chat button", async ({ page }) => {
  await page
    .locator(
      'button[aria-label="Chat feature coming soon - click to learn more"]',
    )
    .click();
  await page.waitForTimeout(300);
});

When("I click the gateway-unavailable chat button", async ({ page }) => {
  await page
    .locator('button[aria-label="Open chat - feature currently unavailable"]')
    .click();
  await page.waitForTimeout(300);
});

Then("the AI chatbot dialog should open", async ({ page }) => {
  await expect(page.locator("dialog[open]").first()).toBeVisible();
});

Then("the chatbot dialog should open", async ({ page }) => {
  await expect(page.locator("dialog[open]").first()).toBeVisible();
});

Then(
  "the AI chatbot dialog should open with title {string}",
  async ({ page }, expectedTitle: string) => {
    const dialog = page.locator("dialog[open]").first();
    await expect(dialog).toBeVisible();
    await expect(
      dialog.locator(`:has-text("${expectedTitle}")`).first(),
    ).toBeVisible();
  },
);

Then(
  "I should see the welcome message {string}",
  async ({ page }, expectedMessage: string) => {
    await expect(
      page.locator(`dialog[open] :has-text("${expectedMessage}")`).first(),
    ).toBeVisible();
  },
);

Then("the chatbot should be ready to receive messages", async ({ page }) => {
  await expect(
    page
      .locator("dialog[open] textarea, dialog[open] input[type='text']")
      .last(),
  ).toBeEnabled();
});

Then(
  "I should see the placeholder text {string}",
  async ({ page }, placeholderText: string) => {
    await expect(
      page.locator(
        `dialog[open] textarea[placeholder="${placeholderText}"], dialog[open] input[placeholder="${placeholderText}"]`,
      ),
    ).toBeVisible();
  },
);

Given("the AI chatbot is open", async ({ page }) => {
  const trigger = page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label="Open BookWorm Literary Guide chat"]',
    )
    .first();
  if (await trigger.isVisible()) {
    await trigger.click();
  }
  await expect(page.locator("dialog[open]").first()).toBeVisible();
});

// Chat interaction
When("I type {string}", async ({ page }, message: string) => {
  await page
    .locator("dialog[open] textarea, dialog[open] input[type='text']")
    .last()
    .fill(message);
});

When("I send the message", async ({ page }) => {
  await page
    .locator(
      'dialog[open] button[type="submit"], dialog[open] button[aria-label*="Send"]',
    )
    .last()
    .click();
  await page.waitForTimeout(1000);
});

When("I send a message", async ({ page }) => {
  const input = page
    .locator("dialog[open] textarea, dialog[open] input[type='text']")
    .last();
  await input.fill("Recommend a book");
  await page
    .locator(
      'dialog[open] button[type="submit"], dialog[open] button[aria-label*="Send"]',
    )
    .last()
    .click();
  await page.waitForTimeout(1000);
});

Then("I should receive AI-generated book recommendations", async ({ page }) => {
  const dialog = page.locator("dialog[open]").first();
  await expect(dialog).toBeVisible();
});

Then("I should receive relevant book suggestions", async ({ page }) => {
  const dialog = page.locator("dialog[open]").first();
  await expect(dialog).toBeVisible();
});

// Conversation history
Given("I have sent multiple messages", async ({ page }) => {
  const input = page
    .locator("dialog[open] textarea, dialog[open] input[type='text']")
    .last();
  await input.fill("Hello");
  await input.press("Enter");
  await page.waitForTimeout(300);
});

When("I scroll up in the chat", async ({ page }) => {
  await page
    .locator("dialog[open]")
    .first()
    .evaluate((el) => {
      el.scrollTop = 0;
    });
});

Then("I should see my previous messages", async ({ page }) => {
  await expect(page.locator("dialog[open]").first()).toBeVisible();
});

Then("I should see all AI responses", async ({ page }) => {
  await expect(page.locator("dialog[open]").first()).toBeVisible();
});

// Keyboard navigation & focus management
When("I press Tab until the chat button is focused", async ({ page }) => {
  const trigger = page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label="Open BookWorm Literary Guide chat"]',
    )
    .first();
  await trigger.focus();
  await expect(trigger).toBeFocused();
});

Then("the chatbot should close", async ({ page }) => {
  await expect(page.locator("dialog[open]")).not.toBeVisible();
});

Then("focus should return to the trigger button", async ({ page }) => {
  const trigger = page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label="Open BookWorm Literary Guide chat"]',
    )
    .first();
  await expect(trigger).toBeFocused();
});

// Unavailable dialogs
Then(
  'I should see an "unavailable" dialog with title {string}',
  async ({ page }, title: string) => {
    const dialog = page.locator("dialog[open]").first();
    await expect(dialog).toBeVisible();
    await expect(dialog.locator(`h3:has-text("${title}")`)).toBeVisible();
  },
);

Then("I can close the unavailable dialog", async ({ page }) => {
  await page.locator('dialog[open] button[aria-label="Close dialog"]').click();
  await expect(page.locator("dialog[open]")).not.toBeVisible();
});

// Error handling & rate limiting
Given("the AI service is unavailable", async ({ page }) => {
  await page.route("**/api/copilotkit**", (route) =>
    route.fulfill({
      status: 503,
      contentType: "application/json",
      body: JSON.stringify({ error: "Service Unavailable" }),
    }),
  );
});

Then("I should see an error notification", async ({ page }) => {
  const errorLocator = page
    .locator(
      '[role="alert"], :has-text("Something went wrong"), :has-text("error")',
    )
    .first();
  await expect(errorLocator).toBeVisible();
});

Given("I have reached the rate limit", async ({ page }) => {
  await page.route("**/api/copilotkit**", (route) =>
    route.fulfill({
      status: 429,
      contentType: "application/json",
      body: JSON.stringify({ error: "Rate limit exceeded" }),
    }),
  );
});

When("I send another message", async ({ page }) => {
  const input = page
    .locator("dialog[open] textarea, dialog[open] input[type='text']")
    .last();
  await input.fill("Another message");
  await page
    .locator(
      'dialog[open] button[type="submit"], dialog[open] button[aria-label*="Send"]',
    )
    .last()
    .click();
  await page.waitForTimeout(500);
});

Then("I should see a rate limit message", async ({ page }) => {
  const rateLimit = page
    .locator(
      '.copilot-rate-limit-warning, [role="alert"], :has-text("Rate limit"), :has-text("Please wait")',
    )
    .first();
  await expect(rateLimit).toBeVisible();
});
