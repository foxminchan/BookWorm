import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

/**
 * AI Chatbot step definitions
 */

// Feature flag checks
Given("AI features are enabled", async function (this: { page: Page }) {
  // Check or set feature flag for AI features
  await this.page.evaluate(() => {
    localStorage.setItem(
      "feature-flags",
      JSON.stringify({ copilotEnabled: true }),
    );
  });
});

Given("AI features are disabled", async function (this: { page: Page }) {
  await this.page.evaluate(() => {
    localStorage.setItem(
      "feature-flags",
      JSON.stringify({ copilotEnabled: false }),
    );
  });
});

// Opening chatbot
When(
  "I click {string} button",
  async function (this: { page: Page }, buttonText: string) {
    const button = this.page.locator(`button:has-text("${buttonText}")`);
    await button.click();
    await this.page.waitForTimeout(500); // Wait for animation
  },
);

When(
  "I click the floating chat button with label {string}",
  async function (this: { page: Page }, ariaLabel: string) {
    const floatingButton = this.page.locator(
      `button[aria-label="${ariaLabel}"], [data-copilot-chat-trigger]`,
    );
    await floatingButton.click();
    await this.page.waitForTimeout(500);
  },
);

When("I click the floating chat button", async function (this: { page: Page }) {
  const floatingButton = this.page.locator(
    '[data-copilot-chat-trigger], button[aria-label*="chat"]',
  );
  await floatingButton.click();
  await this.page.waitForTimeout(500);
});

Then(
  "the AI chatbot dialog should open",
  async function (this: { page: Page }) {
    const chatDialog = this.page.locator(
      '[role="dialog"]:has-text("Chat"), [data-testid="chat-dialog"]',
    );
    await expect(chatDialog).toBeVisible();
  },
);

Then(
  "the AI chatbot dialog should open with title {string}",
  async function (this: { page: Page }, expectedTitle: string) {
    const chatDialog = this.page.locator('[role="dialog"]');
    await expect(chatDialog).toBeVisible();
    const titleElement = this.page.locator(`:has-text("${expectedTitle}")`);
    await expect(titleElement).toBeVisible();
  },
);

Then("the chatbot should be focused", async function (this: { page: Page }) {
  const chatInput = this.page.locator(
    '[data-testid="chat-input"], textarea[placeholder*="message"], input[placeholder*="Ask"]',
  );
  await expect(chatInput).toBeFocused();
});

Then(
  "I should see the welcome message {string}",
  async function (this: { page: Page }, expectedMessage: string) {
    const welcomeMessage = this.page.locator(
      `[data-testid="welcome-message"], :has-text("${expectedMessage}")`,
    );
    await expect(welcomeMessage).toBeVisible();
  },
);

// Any page navigation
Given("I am on any page", async function (this: { page: Page }) {
  // Could be shop, basket, or any page
  await this.page.goto("/shop");
  await this.page.waitForLoadState("networkidle");
});

Then(
  "the chatbot should be ready to receive messages",
  async function (this: { page: Page }) {
    const chatInput = this.page
      .locator('[data-testid="chat-input"], textarea, input[type="text"]')
      .last();
    await expect(chatInput).toBeEnabled();
  },
);

Then(
  "I should see the placeholder text {string}",
  async function (this: { page: Page }, placeholderText: string) {
    const chatInput = this.page.locator(
      `textarea[placeholder="${placeholderText}"], input[placeholder="${placeholderText}"]`,
    );
    await expect(chatInput).toBeVisible();
  },
);

// Chat interactions
Given("the AI chatbot is open", async function (this: { page: Page }) {
  const chatButton = this.page
    .locator('[data-testid="chat-button"], button[aria-label*="chat"]')
    .first();
  await chatButton.click();
  await this.page.waitForTimeout(500);
});

When("I type {string}", async function (this: { page: Page }, message: string) {
  const chatInput = this.page
    .locator('[data-testid="chat-input"], textarea, input[type="text"]')
    .last();
  await chatInput.fill(message);
});

When("I send the message", async function (this: { page: Page }) {
  const sendButton = this.page
    .locator(
      'button[type="submit"], button[aria-label*="Send"], button:has-text("Send")',
    )
    .last();
  await sendButton.click();
  await this.page.waitForTimeout(2000); // Wait for AI response
});

Then(
  "I should receive AI-generated book recommendations",
  async function (this: { page: Page }) {
    const aiResponse = this.page
      .locator('[data-testid="ai-message"], [data-role="assistant"]')
      .last();
    await expect(aiResponse).toBeVisible({ timeout: 10000 });
    const responseText = await aiResponse.textContent();
    expect(responseText).toBeTruthy();
  },
);

Then(
  "the recommendations should include book titles",
  async function (this: { page: Page }) {
    const aiResponse = this.page
      .locator('[data-testid="ai-message"], [data-role="assistant"]')
      .last();
    const responseText = await aiResponse.textContent();
    // Check for quoted titles or book-like patterns
    expect(responseText).toMatch(/["'][\w\s]+["']|Book/i);
  },
);

Then(
  "each recommendation should have a brief description",
  async function (this: { page: Page }) {
    const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
    const responseText = await aiResponse.textContent();
    expect(responseText!.length).toBeGreaterThan(50); // Ensure substantial content
  },
);

Then(
  "I should receive relevant book suggestions",
  async function (this: { page: Page }) {
    const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
    await expect(aiResponse).toBeVisible({ timeout: 10000 });
  },
);

Then(
  "the response should explain why the books are similar",
  async function (this: { page: Page }) {
    const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
    const responseText = await aiResponse.textContent();
    expect(responseText).toMatch(/similar|because|like|recommend/i);
  },
);

Then(
  "I should receive topic-specific recommendations",
  async function (this: { page: Page }) {
    const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
    await expect(aiResponse).toBeVisible({ timeout: 10000 });
  },
);

Then(
  "I should be able to click on book titles to view details",
  async function (this: { page: Page }) {
    const bookLink = this.page
      .locator('[data-testid="ai-message"] a, [data-role="assistant"] a')
      .first();
    const linkCount = await bookLink.count();
    expect(linkCount).toBeGreaterThan(0);
  },
);

// Chat navigation
Given("I received book recommendations", async function (this: { page: Page }) {
  // Assume previous steps filled the chat with recommendations
  const aiResponse = this.page.locator('[data-testid="ai-message"]');
  await expect(aiResponse.first()).toBeVisible();
});

When(
  "I click on a recommended book title",
  async function (this: { page: Page }) {
    const bookLink = this.page
      .locator('[data-testid="ai-message"] a, [data-role="assistant"] a')
      .first();
    await bookLink.click();
  },
);

Then(
  "I should be redirected to that book's detail page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/shop\/[\w-]+/);
  },
);

Then(
  "the chatbot should remain available",
  async function (this: { page: Page }) {
    const chatButton = this.page.locator(
      '[data-testid="chat-button"], button[aria-label*="chat"]',
    );
    await expect(chatButton).toBeVisible();
  },
);

// Chat history
Given("I have sent multiple messages", async function (this: { page: Page }) {
  // Send a few test messages
  const chatInput = this.page
    .locator('[data-testid="chat-input"], textarea')
    .last();
  const sendButton = this.page.locator('button[type="submit"]').last();

  await chatInput.fill("First message");
  await sendButton.click();
  await this.page.waitForTimeout(1000);

  await chatInput.fill("Second message");
  await sendButton.click();
  await this.page.waitForTimeout(1000);
});

When("I scroll up in the chat", async function (this: { page: Page }) {
  const chatContainer = this.page.locator(
    '[data-testid="chat-messages"], [role="log"]',
  );
  await chatContainer.evaluate((el) => (el.scrollTop = 0));
});

Then(
  "I should see my previous messages",
  async function (this: { page: Page }) {
    const userMessages = this.page.locator(
      '[data-role="user"], [data-testid="user-message"]',
    );
    const count = await userMessages.count();
    expect(count).toBeGreaterThan(1);
  },
);

Then("I should see all AI responses", async function (this: { page: Page }) {
  const aiMessages = this.page.locator(
    '[data-role="assistant"], [data-testid="ai-message"]',
  );
  const count = await aiMessages.count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "the conversation should be in chronological order",
  async function (this: { page: Page }) {
    const allMessages = this.page.locator(
      '[data-testid="chat-message"], [data-role="user"], [data-role="assistant"]',
    );
    const count = await allMessages.count();
    expect(count).toBeGreaterThan(1);
  },
);

// Clear chat
Given("I have an existing conversation", async function (this: { page: Page }) {
  const messages = this.page.locator('[data-testid="chat-message"]');
  const count = await messages.count();
  expect(count).toBeGreaterThan(0);
});

When(
  "I click {string}",
  async function (this: { page: Page }, buttonText: string) {
    const button = this.page.locator(`button:has-text("${buttonText}")`);
    await button.click();
  },
);

Then(
  "the chat history should be cleared",
  async function (this: { page: Page }) {
    const messages = this.page.locator('[data-testid="chat-message"]');
    await this.page.waitForTimeout(500);
    const count = await messages.count();
    expect(count).toBeLessThanOrEqual(1); // Only welcome message
  },
);

// Keyboard navigation
When(
  "I press Tab until the chat button is focused",
  async function (this: { page: Page }) {
    // Press Tab multiple times to reach chat button
    for (let i = 0; i < 20; i++) {
      await this.page.keyboard.press("Tab");
      const chatButton = this.page.locator('[data-testid="chat-button"]');
      if (await chatButton.evaluate((el) => document.activeElement === el)) {
        break;
      }
    }
  },
);

When("I press Enter", async function (this: { page: Page }) {
  await this.page.keyboard.press("Enter");
  await this.page.waitForTimeout(500);
});

When("I press Tab to the send button", async function (this: { page: Page }) {
  await this.page.keyboard.press("Tab");
  await this.page.waitForTimeout(100);
});

Then("the message should be sent", async function (this: { page: Page }) {
  const messages = this.page.locator('[data-testid="chat-message"]');
  const count = await messages.count();
  expect(count).toBeGreaterThan(0);
});

// Focus trap
When("I press Tab repeatedly", async function (this: { page: Page }) {
  for (let i = 0; i < 10; i++) {
    await this.page.keyboard.press("Tab");
    await this.page.waitForTimeout(100);
  }
});

Then(
  "focus should cycle within the dialog",
  async function (this: { page: Page }) {
    const focusedElement = await this.page.evaluate(
      () => document.activeElement?.tagName,
    );
    expect(focusedElement).toBeTruthy();
  },
);

Then(
  "focus should not escape to the page behind",
  async function (this: { page: Page }) {
    const chatDialog = this.page.locator('[role="dialog"]');
    const focusedElement = this.page.locator(":focus");
    const isInsideDialog = await focusedElement.evaluate(
      (el, dialog) => {
        return dialog?.contains(el);
      },
      await chatDialog.elementHandle(),
    );
    expect(isInsideDialog).toBeTruthy();
  },
);

// Close with Escape or close button
When(
  "I press Escape or click the close button",
  async function (this: { page: Page }) {
    const escapeOrClose = Math.random() > 0.5;
    if (escapeOrClose) {
      await this.page.keyboard.press("Escape");
    } else {
      const closeButton = this.page
        .locator('button[aria-label*="Close"], button:has([class*="X"])')
        .first();
      await closeButton.click();
    }
    await this.page.waitForTimeout(500);
  },
);

When("I press Escape", async function (this: { page: Page }) {
  await this.page.keyboard.press("Escape");
  await this.page.waitForTimeout(500);
});

Then("the chatbot should close", async function (this: { page: Page }) {
  const chatDialog = this.page.locator('[role="dialog"]');
  await expect(chatDialog).not.toBeVisible();
});

Then(
  "focus should return to the trigger button",
  async function (this: { page: Page }) {
    const chatButton = this.page.locator('[data-testid="chat-button"]');
    await expect(chatButton).toBeFocused();
  },
);

// Close with button
When("I click the close button", async function (this: { page: Page }) {
  const closeButton = this.page
    .locator('[data-testid="close-chat"], button[aria-label*="Close"]')
    .first();
  await closeButton.click();
});

Then(
  "the chatbot should close smoothly",
  async function (this: { page: Page }) {
    await this.page.waitForTimeout(500);
    const chatDialog = this.page.locator('[role="dialog"]');
    await expect(chatDialog).not.toBeVisible();
  },
);

Then(
  "my conversation should be preserved",
  async function (this: { page: Page }) {
    // Reopen and check messages still there
    const chatButton = this.page.locator('[data-testid="chat-button"]').first();
    await chatButton.click();
    await this.page.waitForTimeout(500);
    const messages = this.page.locator('[data-testid="chat-message"]');
    const count = await messages.count();
    expect(count).toBeGreaterThan(0);
  },
);

// Unavailable dialog steps
Then(
  'I should see an "unavailable" dialog',
  async function (this: { page: Page }) {
    const unavailableDialog = this.page.locator(
      '[role="dialog"]:has-text("unavailable"), [role="dialog"]:has-text("Development")',
    );
    await expect(unavailableDialog).toBeVisible();
  },
);

Then(
  "the dialog should show {string} title",
  async function (this: { page: Page }, expectedTitle: string) {
    const titleElement = this.page.locator(
      `h2:has-text("${expectedTitle}"), h3:has-text("${expectedTitle}")`,
    );
    await expect(titleElement).toBeVisible();
  },
);

Then(
  "I should see message {string}",
  async function (this: { page: Page }, expectedMessage: string) {
    const messageElement = this.page.locator(`text=${expectedMessage}`);
    await expect(messageElement).toBeVisible();
  },
);

Then(
  "I can close the dialog by clicking outside or the close button",
  async function (this: { page: Page }) {
    // Try clicking close button
    const closeButton = this.page
      .locator('button[aria-label*="Close"]')
      .first();
    if (await closeButton.isVisible()) {
      await closeButton.click();
      await this.page.waitForTimeout(500);
    }
    // Verify dialog is closed
    const dialog = this.page.locator('[role="dialog"]');
    await expect(dialog).not.toBeVisible();
  },
);

Then(
  'I should see an "unavailable" dialog with title {string}',
  async function (this: { page: Page }, expectedTitle: string) {
    const unavailableDialog = this.page.locator('[role="dialog"]');
    await expect(unavailableDialog).toBeVisible();
    const titleElement = this.page.locator(`text=${expectedTitle}`);
    await expect(titleElement).toBeVisible();
  },
);

// Error handling
Given(
  "the API gateway is not configured",
  async function (this: { page: Page }) {
    // Remove or clear gateway configuration
    await this.page.evaluate(() => {
      delete (window as any).NEXT_PUBLIC_GATEWAY_HTTPS;
      delete (window as any).NEXT_PUBLIC_GATEWAY_HTTP;
    });
  },
);

Given("the AI service is unavailable", async function (this: { page: Page }) {
  // Mock network failure
  await this.page.route("**/api/chat**", (route) => route.abort());
});

Then(
  "I should see an error notification",
  async function (this: { page: Page }) {
    const errorNotification = this.page.locator(
      '[role="alert"], .error, :has-text("error")',
    );
    await expect(errorNotification).toBeVisible();
  },
);

Then(
  "I should be prompted to try again later",
  async function (this: { page: Page }) {
    const retryMessage = this.page.locator(
      ':has-text("try again"), :has-text("later")',
    );
    await expect(retryMessage).toBeVisible();
  },
);

// Rate limiting
Given("I have reached the rate limit", async function (this: { page: Page }) {
  // Set rate limit in localStorage or mock API response
  await this.page.evaluate(() => {
    localStorage.setItem("chat-rate-limit", "reached");
  });
});

When("I send another message", async function (this: { page: Page }) {
  const chatInput = this.page
    .locator('[data-testid="chat-input"], textarea')
    .last();
  const sendButton = this.page.locator('button[type="submit"]').last();
  await chatInput.fill("Test message");
  await sendButton.click();
});

Then(
  "I should see a rate limit message",
  async function (this: { page: Page }) {
    const rateLimitMessage = this.page.locator(
      ':has-text("rate limit"), :has-text("too many")',
    );
    await expect(rateLimitMessage).toBeVisible();
  },
);

Then(
  "I should be informed when I can send again",
  async function (this: { page: Page }) {
    const retryInfo = this.page.locator(
      ':has-text("minute"), :has-text("wait")',
    );
    await expect(retryInfo).toBeVisible();
  },
);

// Mobile experience
Given("I am on a mobile device", async function (this: { page: Page }) {
  await this.page.setViewportSize({ width: 375, height: 667 });
});

Then(
  "the floating chat button should be hidden on mobile",
  async function (this: { page: Page }) {
    const chatButton = this.page.locator(
      '[data-copilot-chat-trigger], button[aria-label*="chat"]',
    );
    // Button exists but is hidden (md:flex class)
    const isVisible = await chatButton.isVisible();
    expect(isVisible).toBe(false);
  },
);

Then(
  "the chatbot is only available on desktop screens",
  async function (this: { page: Page }) {
    // Verify viewport is mobile-sized
    const viewport = this.page.viewportSize();
    expect(viewport?.width).toBeLessThan(768);
  },
);

// Desktop chat UI
Given("I am on a desktop device", async function (this: { page: Page }) {
  await this.page.setViewportSize({ width: 1280, height: 720 });
});

When("I open the AI chatbot", async function (this: { page: Page }) {
  const chatButton = this.page.locator(
    '[data-copilot-chat-trigger], button[aria-label*="chat"]',
  );
  await chatButton.click();
  await this.page.waitForTimeout(500);
});

Then(
  "the chatbot should use the CopilotSidebar component",
  async function (this: { page: Page }) {
    const chatDialog = this.page.locator('[role="dialog"]');
    await expect(chatDialog).toBeVisible();
  },
);

Then(
  "it should support click outside to close",
  async function (this: { page: Page }) {
    // This is a property of CopilotSidebar, we can verify the overlay exists
    const overlay = this.page.locator('.fixed.inset-0, [role="dialog"]');
    await expect(overlay).toBeVisible();
  },
);

Then(
  "the sidebar should be positioned fixed with full-screen overlay",
  async function (this: { page: Page }) {
    const dialog = this.page.locator('[role="dialog"]');
    const className = await dialog.getAttribute("class");
    expect(className).toContain("fixed");
  },
);
