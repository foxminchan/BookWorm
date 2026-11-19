import { expect } from "@playwright/test";

import { forEachSequentially } from "../utils/forEachSequentially";
import { Given, Then, When } from "./fixtures";

/**
 * Account management step definitions.
 * Shared steps defined elsewhere:
 *   "I click {string}" → common.steps.ts
 *   "I navigate to my account page" → authentication.steps.ts
 *   "I should be logged out" → authentication.steps.ts
 *   "I should be redirected to homepage" → authentication.steps.ts
 */

Given("I am on my account page", async ({ accountPage }) => {
  await accountPage.navigate();
});

Then("I should see my account dashboard", async ({ accountPage }) => {
  await expect(accountPage.pageHeading).toBeVisible();
});

Then("I should see my profile information", async ({ accountPage }) => {
  await expect(accountPage.profileSection).toBeVisible();
});

Then("I should see my delivery address section", async ({ accountPage }) => {
  await expect(accountPage.deliveryAddressSection).toBeVisible();
});

Then('I should see the "Order History" link', async ({ accountPage }) => {
  await expect(accountPage.ordersLink).toBeVisible();
});

Then("I should see the account logout button", async ({ accountPage }) => {
  await expect(accountPage.logoutButton).toBeVisible();
});

When(
  'I click "Edit" on the delivery address section',
  async ({ accountPage }) => {
    await accountPage.openEditAddress();
  },
);

When(
  "I fill in the delivery address form:",
  async ({ accountPage }, dataTable: any) => {
    const data: Record<string, string> = dataTable.rowsHash();
    await forEachSequentially(Object.entries(data), async ([field, value]) => {
      if (field === "Street") {
        await accountPage.streetInput.fill(value);
      } else if (field === "City") {
        await accountPage.cityInput.fill(value);
      } else if (field === "Province") {
        await accountPage.provinceInput.fill(value);
      }
    });
  },
);

When("I clear the delivery address fields", async ({ accountPage }) => {
  await accountPage.streetInput.clear();
  await accountPage.cityInput.clear();
  await accountPage.provinceInput.clear();
});

Then(
  "my delivery address should display {string}",
  async ({ accountPage }, expectedAddress: string) => {
    await expect(accountPage.deliveryAddressSection).toContainText(
      expectedAddress,
    );
  },
);

Then("the delivery address form should close", async ({ accountPage }) => {
  await expect(accountPage.streetInput).not.toBeVisible();
  await expect(accountPage.editAddressButton).toBeVisible();
});

Then(
  "the delivery address form should remain open",
  async ({ accountPage }) => {
    await expect(accountPage.streetInput).toBeVisible();
  },
);

When(
  'I click the "Order History" link on account page',
  async ({ accountPage }) => {
    await accountPage.goToOrders();
  },
);

Then(
  "I should be redirected to {string}",
  async ({ page }, expectedPath: string) => {
    await expect(page).toHaveURL(new RegExp(expectedPath));
  },
);

When("I click the account logout button", async ({ accountPage }) => {
  await accountPage.logout();
});
