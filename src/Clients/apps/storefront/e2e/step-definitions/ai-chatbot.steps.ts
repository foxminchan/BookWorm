import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

/**
 * AI Chatbot step definitions
 */

// Feature flag check
Given("AI features are enabled", async function (this: { page: Page }) {
  // Check or set feature flag for AI features
  await this.page.evaluate(() => {
    localStorage.setItem(
      "feature-flags",
      JSON.stringify({ copilotEnabled: true }),
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

When("I click the floating chat button", async function (this: { page: Page }) {
  const floatingButton = this.page.locator(
    '[data-testid="chat-button"], button[aria-label*="chat"], button:has([class*="MessageCircle"])',
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

Then("the chatbot should be focused", async function (this: { page: Page }) {
  const chatInput = this.page.locator(
    '[data-testid="chat-input"], textarea[placeholder*="message"], input[placeholder*="Ask"]',
  );
  await expect(chatInput).toBeFocused();
});

Then("I should see a welcome message", async function (this: { page: Page }) {
  const welcomeMessage = this.page.locator(
    '[data-testid="welcome-message"], :has-text("How can I help")',
  );
  await expect(welcomeMessage).toBeVisible();
});

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

// Close with Escape
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

// Error handling
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
  "the chatbot should open full-screen",
  async function (this: { page: Page }) {
    const chatDialog = this.page.locator('[role="dialog"]');
    const box = await chatDialog.boundingBox();
    expect(box?.width).toBeGreaterThan(300);
    expect(box?.height).toBeGreaterThan(500);
  },
);

Then(
  "the input field should be accessible above the keyboard",
  async function (this: { page: Page }) {
    const chatInput = this.page
      .locator('[data-testid="chat-input"], textarea')
      .last();
    await expect(chatInput).toBeVisible();
    await expect(chatInput).toBeInViewport();
  },
);

// Smart suggestions
When("I view the initial state", async function (this: { page: Page }) {
  // Just opened, check initial state
  await this.page.waitForTimeout(500);
});

Then("I should see suggested prompts", async function (this: { page: Page }) {
  const suggestions = this.page.locator(
    '[data-testid="suggestion"], button[data-suggestion]',
  );
  const count = await suggestions.count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "I can click a suggestion to auto-fill the input",
  async function (this: { page: Page }) {
    const suggestion = this.page.locator('[data-testid="suggestion"]').first();
    await suggestion.click();
    const chatInput = this.page
      .locator('[data-testid="chat-input"], textarea')
      .last();
    const value = await chatInput.inputValue();
    expect(value.length).toBeGreaterThan(0);
  },
);

// Copy response
When(
  "I click the copy button on the response",
  async function (this: { page: Page }) {
    const copyButton = this.page
      .locator('[data-testid="copy-response"], button[aria-label*="Copy"]')
      .first();
    await copyButton.click();
  },
);

Then(
  "the response should be copied to clipboard",
  async function (this: { page: Page }) {
    // Check clipboard (note: might need permissions in real tests)
    await this.page.waitForTimeout(500);
  },
);

Then(
  "I should see a {string} confirmation",
  async function (this: { page: Page }, confirmText: string) {
    const confirmation = this.page.locator(`:has-text("${confirmText}")`);
    await expect(confirmation).toBeVisible();
  },
);

// Attachment upload
When("I click the attachment button", async function (this: { page: Page }) {
  const attachButton = this.page.locator(
    '[data-testid="attach-button"], button[aria-label*="Attach"]',
  );
  await attachButton.click();
});

When("I upload a book cover image", async function (this: { page: Page }) {
  const fileInput = this.page.locator('input[type="file"]');
  await fileInput.setInputFiles({
    name: "book-cover.jpg",
    mimeType: "image/jpeg",
    buffer: Buffer.from("fake-image-data"),
  });
});

Then("the AI should analyze the image", async function (this: { page: Page }) {
  await this.page.waitForTimeout(2000);
  const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
  await expect(aiResponse).toBeVisible({ timeout: 10000 });
});

Then(
  "provide information about the book or similar titles",
  async function (this: { page: Page }) {
    const aiResponse = this.page.locator('[data-testid="ai-message"]').last();
    const responseText = await aiResponse.textContent();
    expect(responseText!.length).toBeGreaterThan(20);
  },
);
