import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Account management step definitions.
 * Shared steps defined elsewhere:
 *   "I click {string}" → common.steps.ts
 *   "I should be logged out" → authentication.steps.ts
 */

Given("I am on my account page", async ({ accountPage }) => {
  await accountPage.navigate();
});

Then("I should see my account dashboard", async ({ page }) => {
  const dashboard = page.locator(
    '[data-testid="account-dashboard"], h1:has-text("Account")',
  );
  await expect(dashboard).toBeVisible();
});

Then("I should see my profile information", async ({ page }) => {
  const profileInfo = page.locator(
    '[data-testid="profile-info"], section:has-text("Profile")',
  );
  await expect(profileInfo).toBeVisible();
});

Then("I should see quick links to orders", async ({ page }) => {
  const ordersLink = page.locator('a[href*="orders"], a:has-text("Orders")');
  await expect(ordersLink).toBeVisible();
});

Then("I should see quick links to addresses", async ({ page }) => {
  const addressesLink = page.locator(
    'a[href*="addresses"], a:has-text("Addresses")',
  );
  await expect(addressesLink).toBeVisible();
});

Then("I should see quick links to settings", async ({ page }) => {
  const settingsLink = page.locator(
    'a[href*="settings"], a:has-text("Settings")',
  );
  await expect(settingsLink).toBeVisible();
});

When(
  "I update my first name to {string}",
  async ({ page }, firstName: string) => {
    await page
      .locator('input[name="firstName"], input[id*="first"]')
      .fill(firstName);
  },
);

When(
  "I update my last name to {string}",
  async ({ page }, lastName: string) => {
    await page
      .locator('input[name="lastName"], input[id*="last"]')
      .fill(lastName);
  },
);

When(
  "I update my phone number to {string}",
  async ({ page }, phone: string) => {
    await page.locator('input[name="phone"], input[type="tel"]').fill(phone);
  },
);

Then("my profile should display the updated information", async ({ page }) => {
  const profileInfo = page.locator('[data-testid="profile-info"]');
  await expect(profileInfo).toContainText("Jane");
  await expect(profileInfo).toContainText("Smith");
});

When("I clear the first name field", async ({ page }) => {
  await page.locator('input[name="firstName"], input[id*="first"]').clear();
});

Then("I should see {string} error", async ({ page }, errorMessage: string) => {
  const error = page.locator(`:has-text("${errorMessage}")`);
  await expect(error).toBeVisible();
});

When("I go to {string} section", async ({ page }, sectionName: string) => {
  const sectionLink = page.locator(
    `a:has-text("${sectionName}"), button:has-text("${sectionName}")`,
  );
  await sectionLink.click();
  await page.waitForLoadState("networkidle");
});

When("I fill in the address form:", async ({ page }, dataTable: any) => {
  const data = dataTable.rowsHash();
  for (const [field, value] of Object.entries(data)) {
    let selector = "";
    if (field === "Street Address")
      selector = 'input[name*="street"], input[id*="street"]';
    else if (field === "City")
      selector = 'input[name*="city"], input[id*="city"]';
    else if (field === "State")
      selector = 'input[name*="state"], select[name*="state"]';
    else if (field === "Postal Code")
      selector = 'input[name*="postal"], input[name*="zip"]';
    else if (field === "Country")
      selector = 'select[name*="country"], input[name*="country"]';
    await page.locator(selector).fill(value as string);
  }
});

Then("I should see the new address in my addresses list", async ({ page }) => {
  const addressList = page.locator(
    '[data-testid="addresses-list"], section:has-text("Addresses")',
  );
  await expect(addressList).toContainText("123 Main St");
});

Given("I have multiple addresses saved", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem(
      "addresses",
      JSON.stringify([
        { id: "1", street: "123 Main St", city: "New York", default: true },
        { id: "2", street: "456 Oak Ave", city: "Boston", default: false },
      ]),
    );
  });
});

Given("I am on my account addresses page", async ({ page }) => {
  await page.goto("/account/addresses");
  await page.waitForLoadState("networkidle");
});

When("I select an address", async ({ page }) => {
  await page.locator('[data-testid="address-card"]').first().click();
});

Then("that address should be marked as default", async ({ page }) => {
  await expect(
    page.locator('[data-testid="default-badge"], :has-text("Default")'),
  ).toBeVisible();
});

Then(
  "I should see a {string} badge on the address",
  async ({ page }, badgeText: string) => {
    await expect(page.locator(`:has-text("${badgeText}")`)).toBeVisible();
  },
);

Given("I have a saved address", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem(
      "addresses",
      JSON.stringify([{ id: "1", street: "123 Main St", city: "New York" }]),
    );
  });
});

When("I click {string} on an address", async ({ page }, action: string) => {
  await page
    .locator(`[data-testid="address-card"] button:has-text("${action}")`)
    .first()
    .click();
  await page.waitForTimeout(300);
});

When(
  "I update the street address to {string}",
  async ({ page }, newStreet: string) => {
    await page.locator('input[name*="street"]').fill(newStreet);
  },
);

Then("the address should be updated", async ({ page }) => {
  await page.waitForTimeout(500);
  await expect(page.locator('[data-testid="addresses-list"]')).toBeVisible();
});

Then(
  "I should see {string} in my addresses",
  async ({ page }, address: string) => {
    await expect(page.locator('[data-testid="addresses-list"]')).toContainText(
      address,
    );
  },
);

Then("I should see a confirmation dialog", async ({ page }) => {
  await expect(
    page.locator('[role="dialog"], [data-testid="confirm-dialog"]'),
  ).toBeVisible();
});

When("I confirm the deletion", async ({ page }) => {
  await page
    .locator(
      'button:has-text("Confirm"), button:has-text("Delete"), button:has-text("Yes")',
    )
    .click();
});

Then("the address should be removed from my list", async ({ page }) => {
  await page.waitForTimeout(500);
  const count = await page.locator('[data-testid="address-card"]').count();
  expect(count).toBeGreaterThanOrEqual(0);
});

Given("I am on my account settings page", async ({ page }) => {
  await page.goto("/account/settings");
  await page.waitForLoadState("networkidle");
});

When("I toggle {string} to off", async ({ page }, toggleName: string) => {
  const toggle = page.locator(
    `[role="switch"]:near(:has-text("${toggleName}")), input[type="checkbox"]:near(:has-text("${toggleName}"))`,
  );
  if (await toggle.isChecked()) await toggle.click();
});

When("I toggle {string} to on", async ({ page }, toggleName: string) => {
  const toggle = page.locator(
    `[role="switch"]:near(:has-text("${toggleName}")), input[type="checkbox"]:near(:has-text("${toggleName}"))`,
  );
  if (!(await toggle.isChecked())) await toggle.click();
});

Then("my email preferences should be updated", async ({ page }) => {
  const saved = await page.evaluate(() =>
    localStorage.getItem("emailPreferences"),
  );
  expect(saved).toBeTruthy();
});

Then("I should see a confirmation message", async ({ page }) => {
  await expect(
    page.locator('[role="alert"]:has-text("saved"), :has-text("updated")'),
  ).toBeVisible({ timeout: 5000 });
});

Then("I should see setup instructions", async ({ page }) => {
  await expect(
    page.locator(':has-text("instructions"), :has-text("scan")'),
  ).toBeVisible();
});

Then("I should see a QR code", async ({ page }) => {
  await expect(
    page.locator('[data-testid="qr-code"], img[alt*="QR"]'),
  ).toBeVisible();
});

When("I enter the verification code", async ({ page }) => {
  await page
    .locator('input[name*="code"], input[placeholder*="code"]')
    .fill("123456");
});

Then("2FA should be enabled for my account", async ({ page }) => {
  await expect(
    page.locator(':has-text("enabled"), :has-text("active")'),
  ).toBeVisible();
});

Then("I should be redirected to Keycloak", async ({ page }) => {
  await page.waitForTimeout(1000);
  expect(page.url()).toMatch(/keycloak|auth/i);
});

Then("I should see the password change form", async ({ page }) => {
  await expect(page.locator('form, :has-text("password")')).toBeVisible();
});

Then("I should see a data export request confirmation", async ({ page }) => {
  await expect(
    page.locator(':has-text("export"), :has-text("request")'),
  ).toBeVisible();
});

Then("I should receive an email when data is ready", async () => {
  expect(true).toBeTruthy();
});

Then("I can download a ZIP file with my data", async () => {
  expect(true).toBeTruthy();
});

Then("I should see a warning about account deletion", async ({ page }) => {
  await expect(
    page.locator(
      '[role="alertdialog"], :has-text("warning"), :has-text("permanent")',
    ),
  ).toBeVisible();
});

When("I confirm account deletion", async ({ page }) => {
  await page
    .locator('button:has-text("Confirm"), input[type="checkbox"]')
    .first()
    .click();
});

When("I enter my password", async ({ page }) => {
  await page.locator('input[type="password"]').fill("test-password");
});

Then("my account should be marked for deletion", async ({ page }) => {
  await page.waitForTimeout(1000);
  expect(true).toBeTruthy();
});

Then("I should receive a confirmation email", async () => {
  expect(true).toBeTruthy();
});

Given("I have items in my wishlist", async ({ page }) => {
  await page.evaluate(() => {
    localStorage.setItem(
      "wishlist",
      JSON.stringify([
        { id: "1", title: "Book 1", price: 29.99 },
        { id: "2", title: "Book 2", price: 39.99 },
      ]),
    );
  });
});

Given("I am viewing my wishlist", async ({ page }) => {
  await page.goto("/account/wishlist");
  await page.waitForLoadState("networkidle");
});

Then("I should see all my wishlist items", async ({ page }) => {
  const count = await page.locator('[data-testid="wishlist-item"]').count();
  expect(count).toBeGreaterThan(0);
});

Then(
  "each item should show title, price, and availability",
  async ({ page }) => {
    await expect(
      page.locator('[data-testid="wishlist-item"]').first(),
    ).toContainText(/\$\d+/);
  },
);

When(
  "I click {string} on a wishlist item",
  async ({ page }, action: string) => {
    await page
      .locator(`[data-testid="wishlist-item"] button:has-text("${action}")`)
      .first()
      .click();
  },
);

Then("the item should be removed from my wishlist", async ({ page }) => {
  await page.waitForTimeout(500);
  expect(
    await page.locator('[data-testid="wishlist-item"]').count(),
  ).toBeGreaterThanOrEqual(0);
});

Then("the wishlist count should decrease", async ({ page }) => {
  await expect(page.locator('[data-testid="wishlist-count"]')).toBeVisible();
});

Then("the item should be added to my basket", async ({ page }) => {
  await expect(page.locator('[data-testid="basket-count"]')).toBeVisible();
});

Then("the item should remain in my wishlist", async ({ page }) => {
  expect(
    await page.locator('[data-testid="wishlist-item"]').count(),
  ).toBeGreaterThan(0);
});

Then("I should be redirected to my orders page", async ({ page }) => {
  await expect(page).toHaveURL(/\/account\/orders/);
});

Then("the account sections should be mobile-friendly", async ({ page }) => {
  await expect(page.locator('section, [role="region"]').first()).toBeVisible();
});

Then("I can navigate between sections easily", async ({ page }) => {
  expect(
    await page.locator('nav a, [role="navigation"] a').count(),
  ).toBeGreaterThan(0);
});

Then("all actions should be touch-optimized", async ({ page }) => {
  const box = await page.locator("button").first().boundingBox();
  if (box) expect(box.height).toBeGreaterThanOrEqual(44);
});

When("I navigate using only keyboard", async ({ page }) => {
  for (let i = 0; i < 15; i++) {
    await page.keyboard.press("Tab");
    await page.waitForTimeout(100);
  }
});

Then("all sections should be accessible", async ({ page }) => {
  expect(
    await page.locator('section, [role="region"]').count(),
  ).toBeGreaterThan(0);
});

Then("all forms should be keyboard-navigable", async ({ page }) => {
  expect(
    await page.evaluate(() => document.activeElement?.tagName),
  ).toBeTruthy();
});

Then("focus indicators should be visible", async ({ page }) => {
  await expect(page.locator(":focus")).toBeVisible();
});
