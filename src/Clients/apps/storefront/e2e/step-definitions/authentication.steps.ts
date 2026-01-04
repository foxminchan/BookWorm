import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { AccountPage, LoginPage, RegisterPage } from "../pages";

/**
 * Authentication step definitions
 */

// Login steps
When(
  "I click {string}",
  async function (this: { page: Page }, linkText: string) {
    const link = this.page
      .locator(`a:has-text("${linkText}"), button:has-text("${linkText}")`)
      .first();
    await link.click();
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "I should be redirected to the login page",
  async function (this: { page: Page }) {
    const loginPage = new LoginPage(this.page);
    await expect(this.page).toHaveURL(/\/login/);
  },
);

Then(
  "I should be redirected to the registration page",
  async function (this: { page: Page }) {
    const registerPage = new RegisterPage(this.page);
    await expect(this.page).toHaveURL(/\/register/);
  },
);

Then(
  "I should see {string} message",
  async function (this: { page: Page }, message: string) {
    const text = this.page.locator(`:has-text("${message}")`);
    await expect(text).toBeVisible();
  },
);

Then(
  "I should be redirected to Keycloak OAuth provider",
  async function (this: { page: Page }) {
    // Wait a bit for redirect to initiate
    await this.page.waitForTimeout(2000);
    // Check if URL contains keycloak or auth related path
    const url = this.page.url();
    expect(url).toMatch(/keycloak|auth|oauth/i);
  },
);

// Protected routes
Given("I am not logged in", async function (this: { page: Page }) {
  // Clear any auth state
  await this.page.context().clearCookies();
  await this.page.evaluate(() => localStorage.clear());
});

When(
  "I navigate to {string}",
  async function (this: { page: Page }, path: string) {
    await this.page.goto(path);
    await this.page.waitForLoadState("networkidle");
  },
);

// Logged in state
Given("I am logged in as a customer", async function (this: { page: Page }) {
  // This would typically involve setting up auth tokens or cookies
  // For now, we'll mock the logged-in state
  // In a real implementation, you'd use your auth setup helper
  await this.page.evaluate(() => {
    localStorage.setItem("auth-token", "mock-token");
    localStorage.setItem(
      "user",
      JSON.stringify({ name: "Test User", email: "test@example.com" }),
    );
  });
});

Then("I should still be logged in", async function (this: { page: Page }) {
  // Check if user menu or logged-in indicator is visible
  const userMenu = this.page.locator(
    '[data-testid="user-menu"], [aria-label*="user"]',
  );
  await expect(userMenu).toBeVisible();
});

// User menu and logout
When("I click the user menu", async function (this: { page: Page }) {
  const userMenu = this.page
    .locator(
      '[data-testid="user-menu"], button[aria-label*="user"], button:has-text("Account")',
    )
    .first();
  await userMenu.click();
});

When(
  "I select {string}",
  async function (this: { page: Page }, option: string) {
    const menuItem = this.page
      .locator(
        `[role="menuitem"]:has-text("${option}"), a:has-text("${option}"), button:has-text("${option}")`,
      )
      .first();
    await menuItem.click();
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should be logged out", async function (this: { page: Page }) {
  // Check that auth token is cleared
  const token = await this.page.evaluate(() =>
    localStorage.getItem("auth-token"),
  );
  expect(token).toBeNull();
});

Then(
  "I should be redirected to homepage",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/^\/$|\/$/);
  },
);

Then(
  "I should see {string} button in header",
  async function (this: { page: Page }, buttonText: string) {
    const button = this.page.locator(
      `header button:has-text("${buttonText}"), header a:has-text("${buttonText}")`,
    );
    await expect(button).toBeVisible();
  },
);

// User icon navigation
When("I click the user icon in header", async function (this: { page: Page }) {
  const userIcon = this.page
    .locator(
      'header [data-testid="user-icon"], header button[aria-label*="user"]',
    )
    .first();
  await userIcon.click();
});

Then("I should see login options", async function (this: { page: Page }) {
  const loginOption = this.page.locator(
    ':has-text("Sign In"), :has-text("Login")',
  );
  await expect(loginOption).toBeVisible();
});

Then("I should be redirected to login", async function (this: { page: Page }) {
  await expect(this.page).toHaveURL(/\/login/);
});

// Session persistence
Then(
  "the user menu should show my account name",
  async function (this: { page: Page }) {
    const user = await this.page.evaluate(() =>
      JSON.parse(localStorage.getItem("user") || "{}"),
    );
    if (user.name) {
      const accountName = this.page.locator(`:has-text("${user.name}")`);
      await expect(accountName).toBeVisible();
    }
  },
);

// Checkout authentication
Given("I have items in my basket", async function (this: { page: Page }) {
  // Add items to basket for testing
  await this.page.evaluate(() => {
    localStorage.setItem("basket", JSON.stringify([{ id: "1", quantity: 2 }]));
  });
});

When("I attempt to checkout", async function (this: { page: Page }) {
  await this.page.goto("/basket");
  const checkoutButton = this.page.locator(
    'button:has-text("Checkout"), a:has-text("Checkout")',
  );
  await checkoutButton.click();
});

Then(
  "after login I should return to checkout",
  async function (this: { page: Page }) {
    // This would be handled by redirect URL after OAuth
    await expect(this.page).toHaveURL(/checkout/);
  },
);

// Token expiry
Given(
  "my authentication token has expired",
  async function (this: { page: Page }) {
    await this.page.evaluate(() => {
      const expiredToken = "expired-token";
      localStorage.setItem("auth-token", expiredToken);
      localStorage.setItem("token-expiry", String(Date.now() - 10000)); // expired 10 seconds ago
    });
  },
);

When("I navigate to my account page", async function (this: { page: Page }) {
  const accountPage = new AccountPage(this.page);
  await accountPage.navigate();
});

Then(
  "I should be prompted to login again",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/login/);
  },
);

// OAuth callback
Given("I initiated OAuth login", async function (this: { page: Page }) {
  await this.page.goto("/login");
  this.page.context().storageState();
});

When(
  "the OAuth provider returns success",
  async function (this: { page: Page }) {
    // Simulate OAuth callback with success
    await this.page.goto("/?code=success-code&state=valid-state");
  },
);

When(
  "the OAuth provider returns an error",
  async function (this: { page: Page }) {
    // Simulate OAuth callback with error
    await this.page.goto("/login?error=access_denied");
  },
);

Then("I should see an error message", async function (this: { page: Page }) {
  const errorMessage = this.page.locator(
    '[role="alert"], .error, :has-text("error")',
  );
  await expect(errorMessage).toBeVisible();
});

Then(
  "I should remain on the login page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/login/);
  },
);
