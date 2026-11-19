import { expect } from "@playwright/test";

import { BASE_PATH, testData } from "../fixtures/test-data";
import { Given, Then, When } from "./fixtures";

/**
 * Common step definitions used across multiple features
 */

// Background steps
Given("the backoffice application is running", async ({ page }) => {
  await page.goto(BASE_PATH);
  await page.waitForLoadState("networkidle");
});

Given("I am logged in as an admin", async ({ page, scenarioData }) => {
  scenarioData.isAuthenticated = true;

  await page.route("**/backoffice/api/auth/get-session*", async (route) => {
    if (scenarioData.isAuthenticated) {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: JSON.stringify({
          session: {
            id: "test-session-id",
            userId: testData.admin.id,
            expiresAt: new Date(Date.now() + 3600 * 1000).toISOString(),
            token: "test-session-token",
          },
          user: {
            id: testData.admin.id,
            name: testData.admin.name,
            email: testData.admin.email,
            emailVerified: true,
          },
        }),
      });
    } else {
      await route.fulfill({
        status: 200,
        contentType: "application/json",
        body: "null",
      });
    }
  });

  await page.route("**/backoffice/api/auth/sign-out*", async (route) => {
    scenarioData.isAuthenticated = false;
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({ success: true }),
    });
  });

  await page.route("**/backoffice/api/auth/sign-in/social*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        url: "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
        redirect: false,
      }),
    });
  });
});

Given("I am not logged in", async ({ page, scenarioData }) => {
  scenarioData.isAuthenticated = false;
  await page.context().clearCookies();

  await page.route("**/backoffice/api/auth/get-session*", async (route) => {
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: "null",
    });
  });

  await page.route("**/backoffice/api/auth/sign-in/social*", async (route) => {
    scenarioData.socialSignInRequested = true;
    await route.fulfill({
      status: 200,
      contentType: "application/json",
      body: JSON.stringify({
        url: "https://login.microsoftonline.com/common/oauth2/v2.0/authorize",
        redirect: false,
      }),
    });
  });
});

// Navigation steps
Given("I am on the homepage", async ({ page }) => {
  await page.goto(BASE_PATH);
  await page.waitForLoadState("networkidle");
});

Given("I am on the dashboard", async ({ dashboardPage }) => {
  await dashboardPage.navigate();
});

Given("I am on the books page", async ({ booksPage }) => {
  await booksPage.navigate();
});

Given("I am on the orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

Given("I am on the customers page", async ({ customersPage }) => {
  await customersPage.navigate();
});

Given("I am on the reviews page", async ({ reviewsPage }) => {
  await reviewsPage.navigate();
});

When("I navigate to the dashboard", async ({ dashboardPage }) => {
  await dashboardPage.navigate();
});

When("I navigate to {string}", async ({ page }, path: string) => {
  const resolved =
    path === BASE_PATH || path.startsWith(`${BASE_PATH}/`)
      ? path
      : `${BASE_PATH}${path.startsWith("/") ? path : `/${path}`}`;
  await page.goto(resolved);
  await page.waitForLoadState("networkidle");
});

When("I navigate to the next page", async ({ page }) => {
  await page.getByRole("button", { name: "Go to next page" }).click();
  await page.waitForLoadState("networkidle");
});

When("I navigate to the previous page", async ({ page }) => {
  await page.getByRole("button", { name: "Go to previous page" }).click();
  await page.waitForLoadState("networkidle");
});

When("I navigate to the books page", async ({ booksPage }) => {
  await booksPage.navigate();
});

When("I navigate to the orders page", async ({ ordersPage }) => {
  await ordersPage.navigate();
});

// Button & link click steps
When("I click {string}", async ({ page }, buttonText: string) => {
  await page.getByRole("button", { name: buttonText }).click();
});

When(
  "I click on {string} in the navigation",
  async ({ page }, linkText: string) => {
    await page
      .locator('nav[aria-label="Main navigation"]')
      .getByRole("link", { name: linkText, exact: true })
      .click();
    await page.waitForLoadState("networkidle");
  },
);

When(
  "I click on {string} in the breadcrumb",
  async ({ page }, linkText: string) => {
    await page
      .locator(`nav[aria-label="Breadcrumb"] a:has-text("${linkText}")`)
      .click();
    await page.waitForLoadState("networkidle");
  },
);

// Assertion steps
Then("I should see {string}", async ({ page }, text: string) => {
  await expect(page.locator(`text=${text}`).first()).toBeVisible();
});

Then(
  "I should see the {string} button",
  async ({ page }, buttonText: string) => {
    await expect(
      page
        .getByRole("button", { name: buttonText })
        .or(page.getByRole("link", { name: buttonText })),
    ).toBeVisible();
  },
);

Then("I should see the navigation menu", async ({ page }) => {
  await expect(page.locator('nav[aria-label="Main navigation"]')).toBeVisible();
});

Then("I should be on the dashboard", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/?$/);
});

Then("I should be on the books page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/books/);
});

Then("I should be on the authors page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/authors/);
});

Then("I should be on the categories page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/categories/);
});

Then("I should be on the publishers page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/publishers/);
});

Then("I should be on the orders page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/orders/);
});

Then("I should be on the customers page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/customers/);
});

Then("I should be on the reviews page", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/reviews/);
});

Then("I should remain authenticated", async ({ page }) => {
  await expect(page.locator('nav[aria-label="Main navigation"]')).toBeVisible();
});

Then("I should see a success message", async ({ page }) => {
  await expect(page.locator("[data-sonner-toast]").first()).toBeVisible();
});

Then("I should see a confirmation dialog", async ({ page }) => {
  await expect(page.locator('[role="alertdialog"]')).toBeVisible();
});

Then("the pagination should update", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

// Data existence checks
Given("the catalog has multiple books", async ({ booksPage }) => {
  await booksPage.navigate();
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});

Given("the catalog has at least one book", async ({ booksPage }) => {
  await booksPage.navigate();
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThanOrEqual(1);
});

Given(
  "the catalog has more than {int} books",
  async ({ booksPage }, bookCount: number) => {
    await booksPage.navigate();
    const count = await booksPage.getBooksCount();
    expect(count).toBeGreaterThan(bookCount);
  },
);
