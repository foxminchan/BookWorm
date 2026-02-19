import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Authentication step definitions
 */

Given("I am on the login page", async ({ loginPage }) => {
  await loginPage.navigate();
});

When("I enter valid admin credentials", async ({ loginPage, credentials }) => {
  await loginPage.fillEmail(credentials.validAdmin.email);
  await loginPage.fillPassword(credentials.validAdmin.password);
});

When(
  "I enter invalid admin credentials",
  async ({ loginPage, credentials }) => {
    await loginPage.fillEmail(credentials.invalidAdmin.email);
    await loginPage.fillPassword(credentials.invalidAdmin.password);
  },
);

When("I click the login button", async ({ loginPage }) => {
  await loginPage.clickLogin();
});

When("I navigate to the login page", async ({ loginPage }) => {
  await loginPage.navigate();
});

When("I open the user menu", async ({ dashboardPage }) => {
  await dashboardPage.openUserMenu();
});

When("I click logout", async ({ page }) => {
  await page.getByRole("button", { name: "Logout" }).click();
  await page.waitForLoadState("networkidle");
});

Then("I should be redirected to the dashboard", async ({ page }) => {
  await expect(page).toHaveURL("/", { timeout: 15000 });
});

Then("I should be redirected to the login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/api\/auth\/signin|\/login/, {
    timeout: 15000,
  });
});

Then(
  "I should be redirected to Keycloak OAuth provider",
  async ({ loginPage }) => {
    await loginPage.waitForOAuthRedirect();
  },
);

Then("I should see an authentication error message", async ({ loginPage }) => {
  const hasError = await loginPage.hasErrorMessage();
  expect(hasError).toBeTruthy();
});

Then("I should remain on the login page", async ({ page }) => {
  await expect(page).toHaveURL(/\/api\/auth\/signin|\/login/);
});

Then("my session should be cleared", async ({ page }) => {
  const cookies = await page.context().cookies();
  const sessionCookies = cookies.filter(
    (cookie) => cookie.name.includes("session") || cookie.name.includes("auth"),
  );
  expect(sessionCookies.length).toBe(0);
});
