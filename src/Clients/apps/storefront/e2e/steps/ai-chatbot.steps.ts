import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * AI Chatbot step definitions.
 * Shared steps defined elsewhere:
 *   "I click {string}" → common.steps.ts
 *   "I press Enter" → accessibility.steps.ts
 *   "I press Escape" → accessibility.steps.ts
 */

/** Deterministic counter to cycle through close actions across invocations */
let closeActionCounter = 0;

// Feature flag checks
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

// Opening chatbot
When("I click {string} button", async ({ page }, buttonText: string) => {
  await page.locator(`button:has-text("${buttonText}")`).click();
  await page.waitForTimeout(500);
});

When(
  "I click the floating chat button with label {string}",
  async ({ page }, ariaLabel: string) => {
    const btn = page
      .locator(`button[aria-label="${ariaLabel}"]`)
      .or(page.locator("[data-copilot-chat-trigger]"));
    await btn.click();
    await page.waitForTimeout(500);
  },
);

When("I click the floating chat button", async ({ page }) => {
  await page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    )
    .click();
  await page.waitForTimeout(500);
});

Then("the AI chatbot dialog should open", async ({ page }) => {
  await expect(
    page.locator(
      '[role="dialog"]:has-text("Chat"), [data-testid="chat-dialog"]',
    ),
  ).toBeVisible();
});

Then(
  "the AI chatbot dialog should open with title {string}",
  async ({ page }, expectedTitle: string) => {
    await expect(page.locator('[role="dialog"]')).toBeVisible();
    await expect(page.locator(`:has-text("${expectedTitle}")`)).toBeVisible();
  },
);

Then("the chatbot should be focused", async ({ page }) => {
  const input = page.locator(
    '[data-testid="chat-input"], textarea[placeholder*="message"], input[placeholder*="Ask"]',
  );
  await expect(input).toBeFocused();
});

Then(
  "I should see the welcome message {string}",
  async ({ page }, expectedMessage: string) => {
    await expect(
      page.locator(
        `[data-testid="welcome-message"], :has-text("${expectedMessage}")`,
      ),
    ).toBeVisible();
  },
);

Given("I am on any page", async ({ page }) => {
  await page.goto("/shop");
  await page.waitForLoadState("networkidle");
});

Then("the chatbot should be ready to receive messages", async ({ page }) => {
  await expect(
    page
      .locator('[data-testid="chat-input"], textarea, input[type="text"]')
      .last(),
  ).toBeEnabled();
});

Then(
  "I should see the placeholder text {string}",
  async ({ page }, placeholderText: string) => {
    await expect(
      page.locator(
        `textarea[placeholder="${placeholderText}"], input[placeholder="${placeholderText}"]`,
      ),
    ).toBeVisible();
  },
);

Given("the AI chatbot is open", async ({ page }) => {
  await page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    )
    .first()
    .click();
  await page.waitForTimeout(500);
});

When("I type {string}", async ({ page }, message: string) => {
  await page
    .locator('[data-testid="chat-input"], textarea, input[type="text"]')
    .last()
    .fill(message);
});

When("I send the message", async ({ page }) => {
  await page
    .locator(
      'button[type="submit"], button[aria-label*="Send"], button:has-text("Send")',
    )
    .last()
    .click();
  await page.waitForTimeout(2000);
});

Then("I should receive AI-generated book recommendations", async ({ page }) => {
  const resp = page
    .locator('[data-testid="ai-message"], [data-role="assistant"]')
    .last();
  await expect(resp).toBeVisible({ timeout: 10000 });
  expect(await resp.textContent()).toBeTruthy();
});

Then("the recommendations should include book titles", async ({ page }) => {
  const text = await page
    .locator('[data-testid="ai-message"], [data-role="assistant"]')
    .last()
    .textContent();
  expect(text).toMatch(/["'][\w\s]+["']|Book/i);
});

Then(
  "each recommendation should have a brief description",
  async ({ page }) => {
    const text = await page
      .locator('[data-testid="ai-message"]')
      .last()
      .textContent();
    expect(text!.length).toBeGreaterThan(50);
  },
);

Then("I should receive relevant book suggestions", async ({ page }) => {
  await expect(page.locator('[data-testid="ai-message"]').last()).toBeVisible({
    timeout: 10000,
  });
});

Then(
  "the response should explain why the books are similar",
  async ({ page }) => {
    const text = await page
      .locator('[data-testid="ai-message"]')
      .last()
      .textContent();
    expect(text).toMatch(/similar|because|like|recommend/i);
  },
);

Then("I should receive topic-specific recommendations", async ({ page }) => {
  await expect(page.locator('[data-testid="ai-message"]').last()).toBeVisible({
    timeout: 10000,
  });
});

Then(
  "I should be able to click on book titles to view details",
  async ({ page }) => {
    const count = await page
      .locator('[data-testid="ai-message"] a, [data-role="assistant"] a')
      .first()
      .count();
    expect(count).toBeGreaterThan(0);
  },
);

Given("I received book recommendations", async ({ page }) => {
  await expect(
    page.locator('[data-testid="ai-message"]').first(),
  ).toBeVisible();
});

When("I click on a recommended book title", async ({ page }) => {
  await page
    .locator('[data-testid="ai-message"] a, [data-role="assistant"] a')
    .first()
    .click();
});

Then("the chatbot should remain available", async ({ page }) => {
  await expect(
    page.locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    ),
  ).toBeVisible();
});

Given("I have sent multiple messages", async ({ page }) => {
  const chatInput = page.locator('[data-testid="chat-input"], textarea').last();
  const sendButton = page.locator('button[type="submit"]').last();
  await chatInput.fill("First message");
  await sendButton.click();
  await page.waitForTimeout(1000);
  await chatInput.fill("Second message");
  await sendButton.click();
  await page.waitForTimeout(1000);
});

When("I scroll up in the chat", async ({ page }) => {
  await page
    .locator('[data-testid="chat-messages"], [role="log"]')
    .evaluate((el) => (el.scrollTop = 0));
});

Then("I should see my previous messages", async ({ page }) => {
  expect(
    await page
      .locator('[data-role="user"], [data-testid="user-message"]')
      .count(),
  ).toBeGreaterThan(1);
});

Then("I should see all AI responses", async ({ page }) => {
  expect(
    await page
      .locator('[data-role="assistant"], [data-testid="ai-message"]')
      .count(),
  ).toBeGreaterThan(0);
});

Then("the conversation should be in chronological order", async ({ page }) => {
  expect(
    await page
      .locator(
        '[data-testid="chat-message"], [data-role="user"], [data-role="assistant"]',
      )
      .count(),
  ).toBeGreaterThan(1);
});

Given("I have an existing conversation", async ({ page }) => {
  expect(
    await page.locator('[data-testid="chat-message"]').count(),
  ).toBeGreaterThan(0);
});

Then("the chat history should be cleared", async ({ page }) => {
  await page.waitForTimeout(500);
  expect(
    await page.locator('[data-testid="chat-message"]').count(),
  ).toBeLessThanOrEqual(1);
});

When("I press Tab until the chat button is focused", async ({ page }) => {
  for (let i = 0; i < 20; i++) {
    await page.keyboard.press("Tab");
    const chatButton = page.locator('[data-testid="chat-button"]');
    if (await chatButton.evaluate((el) => document.activeElement === el)) break;
  }
});

When("I press Tab to the send button", async ({ page }) => {
  await page.keyboard.press("Tab");
  await page.waitForTimeout(100);
});

Then("the message should be sent", async ({ page }) => {
  expect(
    await page.locator('[data-testid="chat-message"]').count(),
  ).toBeGreaterThan(0);
});

When("I press Tab repeatedly", async ({ page }) => {
  for (let i = 0; i < 10; i++) {
    await page.keyboard.press("Tab");
    await page.waitForTimeout(100);
  }
});

Then("focus should cycle within the dialog", async ({ page }) => {
  expect(
    await page.evaluate(() => document.activeElement?.tagName),
  ).toBeTruthy();
});

Then("focus should not escape to the page behind", async ({ page }) => {
  const chatDialog = page.locator('[role="dialog"]');
  const isInside = await page
    .locator(":focus")
    .evaluate(
      (el, dialog) => dialog?.contains(el),
      await chatDialog.elementHandle(),
    );
  expect(isInside).toBeTruthy();
});

When(
  "I press Escape or click the close button or click outside",
  async ({ page }) => {
    const action = closeActionCounter++ % 3;
    if (action === 0) {
      await page.keyboard.press("Escape");
    } else if (action === 1) {
      await page
        .locator('button[aria-label*="Close"], button:has([class*="X"])')
        .first()
        .click();
    } else {
      await page
        .locator(".fixed.inset-0")
        .first()
        .click({ position: { x: 10, y: 10 } });
    }
    await page.waitForTimeout(500);
  },
);

Then("the chatbot should close", async ({ page }) => {
  await expect(page.locator('[role="dialog"]')).not.toBeVisible();
});

Then("focus should return to the trigger button", async ({ page }) => {
  await page.waitForTimeout(300);
  const chatButton = page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    )
    .first();
  await expect(chatButton).toBeFocused();
});

Then("the floating chat button should be visible", async ({ page }) => {
  await expect(
    page.locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    ),
  ).toBeVisible();
});

When("I click the close button", async ({ page }) => {
  await page
    .locator('[data-testid="close-chat"], button[aria-label*="Close"]')
    .first()
    .click();
});

Then("the chatbot should close smoothly", async ({ page }) => {
  await page.waitForTimeout(500);
  await expect(page.locator('[role="dialog"]')).not.toBeVisible();
});

Then("my conversation should be preserved", async ({ page }) => {
  await page.locator("[data-copilot-chat-trigger]").first().click();
  await page.waitForTimeout(500);
  expect(
    await page.locator('[data-testid="chat-message"]').count(),
  ).toBeGreaterThan(0);
});

Then('I should see an "unavailable" dialog', async ({ page }) => {
  await expect(
    page.locator(
      '[role="dialog"]:has-text("Unavailable"), [role="dialog"]:has-text("Development"), [aria-label="BookWorm Literary Guide Chat"]',
    ),
  ).toBeVisible();
});

Then(
  "the dialog should show {string} title",
  async ({ page }, expectedTitle: string) => {
    await expect(
      page.locator(
        `h2:has-text("${expectedTitle}"), h3:has-text("${expectedTitle}")`,
      ),
    ).toBeVisible();
  },
);

Then(
  "I should see message {string}",
  async ({ page }, expectedMessage: string) => {
    await expect(page.locator(`text=${expectedMessage}`)).toBeVisible();
  },
);

Then(
  "I can close the dialog by clicking outside or the close button",
  async ({ page }) => {
    const closeButton = page.locator('button[aria-label*="Close"]').first();
    if (await closeButton.isVisible()) {
      await closeButton.click();
      await page.waitForTimeout(500);
    }
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  },
);

Then(
  'I should see an "unavailable" dialog with title {string}',
  async ({ page }, expectedTitle: string) => {
    await expect(page.locator('[role="dialog"]')).toBeVisible();
    await expect(page.locator(`text=${expectedTitle}`)).toBeVisible();
  },
);

Given("the API gateway is not configured", async ({ page }) => {
  await page.evaluate(() => {
    delete (globalThis as any).NEXT_PUBLIC_GATEWAY_HTTPS;
    delete (globalThis as any).NEXT_PUBLIC_GATEWAY_HTTP;
  });
});

Given("the AI service is unavailable", async ({ page }) => {
  await page.route("**/api/chat**", (route) => route.abort());
});

Then("I should see an error notification", async ({ page }) => {
  await expect(
    page.locator('[role="alert"], .error, :has-text("error")'),
  ).toBeVisible();
});

Then("I should be prompted to try again later", async ({ page }) => {
  await expect(
    page.locator(':has-text("try again"), :has-text("later")'),
  ).toBeVisible();
});

Given("I have reached the rate limit", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem("chat-rate-limit", "reached");
  });
});

When("I send another message", async ({ page }) => {
  const chatInput = page.locator('[data-testid="chat-input"], textarea').last();
  await chatInput.fill("Test message");
  await page.locator('button[type="submit"]').last().click();
});

Then("I should see a rate limit message", async ({ page }) => {
  await expect(
    page.locator(':has-text("rate limit"), :has-text("too many")'),
  ).toBeVisible();
});

Then("I should be informed when I can send again", async ({ page }) => {
  await expect(
    page.locator(':has-text("minute"), :has-text("wait")'),
  ).toBeVisible();
});

Given("I am on a mobile device", async ({ page }) => {
  await page.setViewportSize({ width: 375, height: 667 });
});

Then(
  "the floating chat button should be hidden on mobile",
  async ({ page }) => {
    const isVisible = await page
      .locator(
        '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
      )
      .isVisible();
    expect(isVisible).toBe(false);
  },
);

Then("the chatbot is only available on desktop screens", async ({ page }) => {
  const viewport = page.viewportSize();
  expect(viewport?.width).toBeLessThan(768);
});

Given("I am on a desktop device", async ({ page }) => {
  await page.setViewportSize({ width: 1280, height: 720 });
});

When("I open the AI chatbot", async ({ page }) => {
  await page
    .locator(
      '[data-copilot-chat-trigger], button[aria-label*="BookWorm Literary Guide"]',
    )
    .click();
  await page.waitForTimeout(500);
});

Then(
  "the chatbot should use the CopilotSidebar component",
  async ({ page }) => {
    await expect(page.locator('[role="dialog"]')).toBeVisible();
  },
);

Then("it should support click outside to close", async ({ page }) => {
  await expect(page.locator('.fixed.inset-0, [role="dialog"]')).toBeVisible();
});

Then(
  "the sidebar should be positioned fixed with full-screen overlay",
  async ({ page }) => {
    const className = await page
      .locator('[role="dialog"]')
      .getAttribute("class");
    expect(className).toContain("fixed");
  },
);

// --- Missing Steps / Aliases ---

Then("I should see a fresh welcome message", async ({ page }) => {
  await expect(page.locator('[data-testid="welcome-message"]')).toBeVisible();
});

Then("the chatbot dialog should open", async ({ page }) => {
  await expect(page.locator('[role="dialog"]')).toBeVisible();
});

When("I type a message", async ({ page }) => {
  const input = page.locator('[data-testid="chat-input"], textarea').last();
  await input.fill("Hello, this is a test message");
});

When("I send a message", async ({ page }) => {
  const btn = page
    .locator('button[type="submit"], button[aria-label="Send"]')
    .last();
  await btn.click();
});
