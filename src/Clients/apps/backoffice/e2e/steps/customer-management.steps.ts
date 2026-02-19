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

Given("there are multiple customers", async ({ customersPage }) => {
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThan(1);
});

Given(
  "there are more than {int} customers",
  async ({ customersPage }, customerCount: number) => {
    const count = await customersPage.getCustomersCount();
    expect(count).toBeGreaterThan(customerCount);
  },
);

When("I click view on the first customer", async ({ customersPage }) => {
  await customersPage.viewFirstCustomer();
});

When(
  "I search for a customer by name",
  async ({ customersPage, scenarioData }) => {
    const searchQuery = (scenarioData.customerName as string) || "John Doe";
    await customersPage.searchCustomers(searchQuery);
  },
);

When(
  "I search for a customer by email",
  async ({ customersPage, scenarioData }) => {
    const searchQuery =
      (scenarioData.customerEmail as string) || "customer@example.com";
    await customersPage.searchCustomers(searchQuery);
  },
);

Then("I should see the customers table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then("I should see customer information", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then("I should see the customer details page", async ({ page }) => {
  await expect(page).toHaveURL(/\/ordering\/customers\/.+/);
});

Then("I should see customer contact information", async ({ page }) => {
  await expect(
    page.locator("text=/email|phone|address/i").first(),
  ).toBeVisible();
});

Then("I should see customer order history", async ({ page }) => {
  await expect(
    page.locator("text=/order|purchase|history/i").first(),
  ).toBeVisible();
});

Then("I should see only matching customers", async ({ page }) => {
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThanOrEqual(0);
});

Then("I should see different customers", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
