import { expect } from "@playwright/test";

import { Given, Then, When } from "./fixtures";

/**
 * Customer management step definitions
 */

When("the customers page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Given("there is at least one customer", async ({ customersPage }) => {
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given(
  "there are more than {int} customers",
  async ({ customersPage }, customerCount: number) => {
    const count = await customersPage.getCustomersCount();
    expect(count).toBeGreaterThan(customerCount);
  },
);

When(
  "I click the delete button for the first customer",
  async ({ customersPage }) => {
    await customersPage.deleteFirstCustomer();
  },
);

When("I confirm the customer deletion", async ({ customersPage }) => {
  await customersPage.confirmDelete();
});

When("I cancel the customer deletion", async ({ customersPage }) => {
  await customersPage.cancelDelete();
});

Then("I should see the customers table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then("I should see customer Name and Address columns", async ({ page }) => {
  await expect(
    page.locator("table thead th", { hasText: "Name" }),
  ).toBeVisible();
  await expect(
    page.locator("table thead th", { hasText: "Address" }),
  ).toBeVisible();
});

Then("the customer should remain in the list", async ({ customersPage }) => {
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThan(0);
});

Then("I should see different customers", async ({ customersPage }) => {
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThan(0);
});
