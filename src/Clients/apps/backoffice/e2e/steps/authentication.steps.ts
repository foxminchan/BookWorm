import { expect } from "@playwright/test";

import { Then, When } from "./fixtures";

/**
 * Authentication step definitions
 */

When("I open the user menu", async ({ dashboardPage }) => {
  await dashboardPage.openUserMenu();
});

When("I click log out in the user menu", async ({ dashboardPage }) => {
  await dashboardPage.clickLogoutMenuItem();
});

When("I click the sidebar logout button", async ({ dashboardPage }) => {
  await dashboardPage.logoutFromSidebar();
});

Then(
  "I should see the authentication required prompt",
  async ({ loginPage }) => {
    await loginPage.assertAuthRequiredDisplayed();
  },
);

Then(
  "Microsoft Entra ID OAuth sign-in should be triggered",
  ({ scenarioData }) => {
    expect(scenarioData.socialSignInRequested).toBeTruthy();
  },
);

Then("my session should be cleared", ({ scenarioData }) => {
  expect(scenarioData.isAuthenticated).toBeFalsy();
});
