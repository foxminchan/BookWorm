import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { credentials } from "../fixtures/test-data";
import { DashboardPage, LoginPage } from "../pages";

/**
 * Authentication step definitions
 */

Given("I am on the login page", async function (this: { page: Page }) {
  const loginPage = new LoginPage(this.page);
  await loginPage.navigate();
});

When("I enter valid admin credentials", async function (this: { page: Page }) {
  const loginPage = new LoginPage(this.page);
  await loginPage.fillEmail(credentials.validAdmin.email);
  await loginPage.fillPassword(credentials.validAdmin.password);
});

When(
  "I enter invalid admin credentials",
  async function (this: { page: Page }) {
    const loginPage = new LoginPage(this.page);
    await loginPage.fillEmail(credentials.invalidAdmin.email);
    await loginPage.fillPassword(credentials.invalidAdmin.password);
  },
);

When("I click the login button", async function (this: { page: Page }) {
  const loginPage = new LoginPage(this.page);
  await loginPage.clickLogin();
});

When("I navigate to the login page", async function (this: { page: Page }) {
  const loginPage = new LoginPage(this.page);
  await loginPage.navigate();
});

When("I open the user menu", async function (this: { page: Page }) {
  const dashboardPage = new DashboardPage(this.page);
  await dashboardPage.openUserMenu();
});

When("I click logout", async function (this: { page: Page }) {
  await this.page.getByRole("button", { name: "Logout" }).click();
  await this.page.waitForLoadState("networkidle");
});

Then(
  "I should be redirected to the dashboard",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL("/", { timeout: 15000 });
    const dashboardPage = new DashboardPage(this.page);
    await dashboardPage.assertIsDashboardPage();
  },
);

Then(
  "I should be redirected to the login page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/api\/auth\/signin|\/login/, {
      timeout: 15000,
    });
  },
);

Then(
  "I should be redirected to Keycloak OAuth provider",
  async function (this: { page: Page }) {
    const loginPage = new LoginPage(this.page);
    await loginPage.waitForOAuthRedirect();
  },
);

Then(
  "I should see an authentication error message",
  async function (this: { page: Page }) {
    const loginPage = new LoginPage(this.page);
    const hasError = await loginPage.hasErrorMessage();
    expect(hasError).toBeTruthy();
  },
);

Then(
  "I should remain on the login page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/api\/auth\/signin|\/login/);
  },
);

Then("my session should be cleared", async function (this: { page: Page }) {
  // Verify cookies are cleared
  const cookies = await this.page.context().cookies();
  const sessionCookies = cookies.filter(
    (cookie) => cookie.name.includes("session") || cookie.name.includes("auth"),
  );
  expect(sessionCookies.length).toBe(0);
});
