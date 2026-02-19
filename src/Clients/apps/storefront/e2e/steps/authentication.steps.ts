import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Authentication step definitions.
 * "I click {string}" is defined in common.steps.ts.
 * "I should see {string} message" is defined in common.steps.ts.
 */

// Login redirects
Then("I should be redirected to the login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

Then("I should be redirected to the registration page", async ({ page }) => {
  await expect(page).toHaveURL(/\/register/);
});

Then("I should be redirected to Keycloak OAuth provider", async ({ page }) => {
  await page.waitForTimeout(2000);
  const url = page.url();
  expect(url).toMatch(/keycloak|auth|oauth/i);
});

// Protected routes
Given("I am not logged in", async ({ page }) => {
  await page.context().clearCookies();
  await page.evaluate(() => localStorage.clear());
});

When("I navigate to {string}", async ({ page }, path: string) => {
  await page.goto(path);
  await page.waitForLoadState("networkidle");
});

// Logged in state
Given("I am logged in as a customer", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem("auth-token", "mock-token");
    localStorage.setItem(
      "user",
      JSON.stringify({ name: "Test User", email: "test@example.com" }),
    );
  });
});

Then("I should still be logged in", async ({ page }) => {
  const userMenu = page.locator(
    '[data-testid="user-menu"], [aria-label*="user"]',
  );
  await expect(userMenu).toBeVisible();
});

// User menu and logout
When("I click the user menu", async ({ page }) => {
  const userMenu = page
    .locator(
      '[data-testid="user-menu"], button[aria-label*="user"], button:has-text("Account")',
    )
    .first();
  await userMenu.click();
});

When("I select {string}", async ({ page }, option: string) => {
  const menuItem = page
    .locator(
      `[role="menuitem"]:has-text("${option}"), a:has-text("${option}"), button:has-text("${option}")`,
    )
    .first();
  await menuItem.click();
  await page.waitForLoadState("networkidle");
});

Then("I should be logged out", async ({ page }) => {
  const token = await page.evaluate(() => localStorage.getItem("auth-token"));
  expect(token).toBeNull();
});

Then("I should be redirected to homepage", async ({ page }) => {
  await expect(page).toHaveURL(/^\/$|\/$/);
});

Then(
  "I should see {string} button in header",
  async ({ page }, buttonText: string) => {
    const button = page.locator(
      `header button:has-text("${buttonText}"), header a:has-text("${buttonText}")`,
    );
    await expect(button).toBeVisible();
  },
);

// User icon navigation
When("I click the user icon in header", async ({ page }) => {
  const userIcon = page
    .locator(
      'header [data-testid="user-icon"], header button[aria-label*="user"]',
    )
    .first();
  await userIcon.click();
});

Then("I should see login options", async ({ page }) => {
  const loginOption = page.locator(':has-text("Sign In"), :has-text("Login")');
  await expect(loginOption).toBeVisible();
});

Then("I should be redirected to login", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

// Session persistence
Then("the user menu should show my account name", async ({ page }) => {
  const user = await page.evaluate(() =>
    JSON.parse(localStorage.getItem("user") || "{}"),
  );
  if (user.name) {
    const accountName = page.locator(`:has-text("${user.name}")`);
    await expect(accountName).toBeVisible();
  }
});

// Checkout authentication
Given("I have items in my basket", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem("basket", JSON.stringify([{ id: "1", quantity: 2 }]));
  });
});

When("I attempt to checkout", async ({ page }) => {
  await page.goto("/basket");
  const checkoutButton = page.locator(
    'button:has-text("Checkout"), a:has-text("Checkout")',
  );
  await checkoutButton.click();
});

Then("after login I should return to checkout", async ({ page }) => {
  await expect(page).toHaveURL(/checkout/);
});

// Token expiry
Given("my authentication token has expired", async ({ page }) => {
  await page.evaluate(() => {
    const expiredToken = "expired-token";
    localStorage.setItem("auth-token", expiredToken);
    localStorage.setItem("token-expiry", String(Date.now() - 10000));
  });
});

When("I navigate to my account page", async ({ accountPage }) => {
  await accountPage.navigate();
});

Then("I should be prompted to login again", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

// OAuth callback
Given("I initiated OAuth login", async ({ page }) => {
  await page.goto("/login");
  page.context().storageState();
});

When("the OAuth provider returns success", async ({ page }) => {
  await page.goto("/?code=success-code&state=valid-state");
});

When("the OAuth provider returns an error", async ({ page }) => {
  await page.goto("/login?error=access_denied");
});

Then("I should see an error message", async ({ page }) => {
  const errorMessage = page.locator(
    '[role="alert"], .error, :has-text("error")',
  );
  await expect(errorMessage).toBeVisible();
});

Then("I should remain on the login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

// --- Missing Steps / Aliases ---

Then("I should be redirected to login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

Then("I should be redirected to the homepage", async ({ page }) => {
  await expect(page).toHaveURL(/\/$/);
});

Then("I should be logged in", async ({ page }) => {
  await expect(
    page.locator('button:has-text("Account"), button[aria-label="User menu"]'),
  ).toBeVisible();
});

When("I navigate to the basket page", async ({ basketPage }) => {
  await basketPage.navigate();
});

Given("I am logged in", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem("auth-token", "mock-token");
    localStorage.setItem(
      "user",
      JSON.stringify({ name: "Test User", email: "test@example.com" }),
    );
  });
});
