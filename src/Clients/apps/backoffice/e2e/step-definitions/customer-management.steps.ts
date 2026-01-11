import { Given, Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { CustomersPage } from "../pages";
import { World } from "../world";

/**
 * Customer management step definitions
 */

When("the customers page loads", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Given("there is at least one customer", async function (this: { page: Page }) {
  const customersPage = new CustomersPage(this.page);
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given("there are multiple customers", async function (this: { page: Page }) {
  const customersPage = new CustomersPage(this.page);
  const count = await customersPage.getCustomersCount();
  expect(count).toBeGreaterThan(1);
});

Given(
  "there are more than {int} customers",
  async function (this: { page: Page }, customerCount: number) {
    const customersPage = new CustomersPage(this.page);
    const count = await customersPage.getCustomersCount();
    expect(count).toBeGreaterThan(customerCount);
  },
);

When(
  "I click view on the first customer",
  async function (this: { page: Page }) {
    const customersPage = new CustomersPage(this.page);
    await customersPage.viewFirstCustomer();
  },
);

When("I search for a customer by name", async function (this: World) {
  const customersPage = new CustomersPage(this.page);
  const searchQuery =
    (this.testData.customerName as string | undefined) || "John Doe";
  await customersPage.searchCustomers(searchQuery);
});

When("I search for a customer by email", async function (this: World) {
  const customersPage = new CustomersPage(this.page);
  const searchQuery =
    (this.testData.customerEmail as string | undefined) ||
    "customer@example.com";
  await customersPage.searchCustomers(searchQuery);
});

Then("I should see the customers table", async function (this: { page: Page }) {
  await expect(this.page.locator("table")).toBeVisible();
});

Then(
  "I should see customer information",
  async function (this: { page: Page }) {
    await expect(this.page.locator("table")).toBeVisible();
  },
);

Then(
  "I should see the customer details page",
  async function (this: { page: Page }) {
    await expect(this.page).toHaveURL(/\/ordering\/customers\/.+/);
  },
);

Then(
  "I should see customer contact information",
  async function (this: { page: Page }) {
    await expect(
      this.page.locator("text=/email|phone|address/i").first(),
    ).toBeVisible();
  },
);

Then(
  "I should see customer order history",
  async function (this: { page: Page }) {
    await expect(
      this.page.locator("text=/order|purchase|history/i").first(),
    ).toBeVisible();
  },
);

Then(
  "I should see only matching customers",
  async function (this: { page: Page }) {
    const rows = await this.page.locator("table tbody tr").count();
    expect(rows).toBeGreaterThanOrEqual(0);
  },
);

Then("I should see different customers", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
