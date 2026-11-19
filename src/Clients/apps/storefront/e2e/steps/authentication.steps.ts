import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Authentication step definitions.
 * Uses Better Auth session cookie (`better-auth.session_token`) and `/api/auth/get-session`
 * matching `proxy.ts` and `lib/auth-client.ts`.
 */

const MOCK_USER = {
  id: "test-user-id",
  name: "Test User",
  email: "test@example.com",
  emailVerified: true,
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

const MOCK_SESSION = {
  id: "test-session-id",
  userId: MOCK_USER.id,
  token: "mock-session-token",
  expiresAt: new Date(Date.now() + 86400 * 1000).toISOString(),
  createdAt: new Date().toISOString(),
  updatedAt: new Date().toISOString(),
};

async function setupAuthenticatedSession(
  page: import("@playwright/test").Page,
): Promise<void> {
  let isSignedIn = true;

  await page.context().addCookies([
    {
      name: "better-auth.session_token",
      value: "mock-session-token",
      domain: "localhost",
      path: "/",
    },
  ]);

  await page.route("**/api/auth/get-session*", async (route) => {
    if (!isSignedIn) {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify(null),
      });
      return;
    }

    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        user: MOCK_USER,
        session: MOCK_SESSION,
      }),
    });
  });

  await page.route("**/api/auth/sign-out*", async (route) => {
    isSignedIn = false;
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      headers: {
        "Set-Cookie":
          "better-auth.session_token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT",
      },
      body: JSON.stringify({ success: true }),
    });
  });

  await page.reload();
  await page.waitForLoadState("networkidle");
}

// Login & registration redirects
Then("I should be redirected to the login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

Then("I should be redirected to the registration page", async ({ page }) => {
  await expect(page).toHaveURL(/\/register/);
});

Then(
  "I should be redirected to Microsoft Entra ID OAuth provider",
  async ({ page }) => {
    await page.waitForTimeout(2000);
    const url = page.url();
    expect(url).toMatch(/login\.microsoftonline\.com|auth|oauth/i);
  },
);

// Protected routes & guest state
Given("I am not logged in", async ({ page }) => {
  await page.context().clearCookies();
  await page.route("**/api/auth/get-session*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify(null),
    });
  });
  await page.reload();
  await page.waitForLoadState("networkidle");
});

When("I navigate to {string}", async ({ page }, path: string) => {
  await page.goto(path);
  await page.waitForLoadState("networkidle");
});

Then("the basket icon should not be visible in header", async ({ page }) => {
  await expect(page.locator('header a[href="/basket"]')).not.toBeVisible();
});

// Logged in state
Given("I am logged in as a customer", async ({ page }) => {
  await setupAuthenticatedSession(page);
});

Given("I am logged in", async ({ page }) => {
  await setupAuthenticatedSession(page);
});

Then("I should still be logged in", async ({ page }) => {
  await expect(page.locator('header a[href="/basket"]').first()).toBeVisible();
});

Then("I should be logged in", async ({ page }) => {
  await expect(page.locator('header a[href="/basket"]').first()).toBeVisible();
});

// Account menu (hover-triggered in HeaderAccountMenu)
When("I open the account menu", async ({ homePage }) => {
  await homePage.header.openAccountMenu();
});

When("I select {string}", async ({ page }, option: string) => {
  const menuItem = page
    .locator(
      `[role="menu"][aria-label="Account menu"] [role="menuitem"]:has-text("${option}")`,
    )
    .first();
  await menuItem.click();
  await page.waitForLoadState("networkidle");
});

Then("I should see login options", async ({ page }) => {
  const menu = page.locator('[role="menu"][aria-label="Account menu"]');
  await expect(menu.locator('a:has-text("Login")')).toBeVisible();
  await expect(menu.locator('a:has-text("Register")')).toBeVisible();
});

Then(
  "the account menu should show authenticated options",
  async ({ homePage, page }) => {
    await homePage.header.openAccountMenu();
    const menu = page.locator('[role="menu"][aria-label="Account menu"]');
    await expect(menu.locator('a:has-text("My Profile")')).toBeVisible();
    await expect(menu.locator('a:has-text("Order History")')).toBeVisible();
    await expect(menu.locator('button:has-text("Logout")')).toBeVisible();
  },
);

Then("I should be logged out", async ({ page }) => {
  const cookies = await page.context().cookies();
  const sessionCookie = cookies.find(
    (c) => c.name === "better-auth.session_token" && c.value !== "",
  );
  expect(sessionCookie).toBeUndefined();
});

Then("I should be redirected to homepage", async ({ page }) => {
  await expect(page).toHaveURL(/\/$/);
});

Then("I should be redirected to the homepage", async ({ page }) => {
  await expect(page).toHaveURL(/\/$/);
});

Then(
  "I should see {string} option in account menu",
  async ({ homePage, page }, optionText: string) => {
    await homePage.header.openAccountMenu();
    const option = page
      .locator(
        `[role="menu"][aria-label="Account menu"] [role="menuitem"]:has-text("${optionText}")`,
      )
      .first();
    await expect(option).toBeVisible();
  },
);

Then("I should be redirected to login", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

Then("I should be redirected to login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/login/);
});

When("I navigate to the basket page", async ({ basketPage }) => {
  await basketPage.navigate();
});

When("I navigate to my account page", async ({ accountPage }) => {
  await accountPage.navigate();
});
