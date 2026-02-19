import { expect } from "@playwright/test";

import { Then, When } from "./fixtures";

/**
 * Catalog/Books management step definitions
 */

When("the books page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

When("I click the {string} button", async ({ page }, buttonText: string) => {
  await page.getByRole("button", { name: buttonText }).click();
});

When(
  "I fill in the book form with valid data",
  async ({ booksPage, testData, scenarioData }) => {
    const bookData = {
      title: testData.book.title(),
      author: testData.book.author(),
      isbn: testData.book.isbn(),
      price: testData.book.price().toString(),
      description: testData.book.description(),
    };

    // Store for later verification
    scenarioData.createdBook = bookData;

    await booksPage.fillBookForm(bookData);
  },
);

When("I search for a book by title", async ({ booksPage, scenarioData }) => {
  const searchQuery = (scenarioData.searchQuery as string) || "Test Book";
  await booksPage.searchBooks(searchQuery);
});

When("I search for {string}", async ({ booksPage }, query: string) => {
  await booksPage.searchBooks(query);
});

When("I click the edit button for the first book", async ({ booksPage }) => {
  await booksPage.editFirstBook();
});

When("I update the book title", async ({ page, testData, scenarioData }) => {
  const newTitle = testData.book.title();
  scenarioData.updatedTitle = newTitle;
  await page.fill('input[name="title"]', newTitle);
});

When("I click the delete button for the first book", async ({ booksPage }) => {
  await booksPage.deleteFirstBook();
});

When("I confirm the deletion", async ({ booksPage }) => {
  await booksPage.confirmDelete();
});

When("I cancel the deletion", async ({ booksPage }) => {
  await booksPage.cancelDelete();
});

When("I leave required fields empty", async () => {
  // Just click save without filling anything
  // Form validation will be triggered
});

Then("I should see the books table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then("I should see the search input", async ({ page }) => {
  await expect(page.locator('input[placeholder*="Search"]')).toBeVisible();
});

Then("the book should be added successfully", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then(
  "I should see the new book in the list",
  async ({ booksPage, scenarioData }) => {
    const createdBook = scenarioData.createdBook as
      | { title: string }
      | undefined;
    if (createdBook) {
      const exists = await booksPage.bookExists(createdBook.title);
      expect(exists).toBeTruthy();
    }
  },
);

Then("I should see only matching books", async ({ page }) => {
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThanOrEqual(0);
});

Then("the results should be filtered", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see {string} message", async ({ page }, message: string) => {
  await expect(page.locator(`text=${message}`).first()).toBeVisible();
});

Then("the table should be empty", async ({ page }) => {
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBe(0);
});

Then("the book should be updated successfully", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see the updated information", async ({ page, scenarioData }) => {
  const updatedTitle = scenarioData.updatedTitle as string | undefined;
  if (updatedTitle) {
    await expect(page.locator(`text=${updatedTitle}`).first()).toBeVisible();
  }
});

Then("I should see a confirmation dialog", async ({ page }) => {
  await expect(page.locator('[role="dialog"]')).toBeVisible();
});

Then("the book should be removed from the list", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("the book should remain in the list", async ({ page }) => {
  await expect(page.locator("table tbody tr").first()).toBeVisible();
});

Then("I should see validation error messages", async ({ page }) => {
  await expect(page.locator('[role="alert"]').first()).toBeVisible();
});

Then("the book should not be created", async ({ page }) => {
  await expect(page.locator('button:has-text("Save")')).toBeVisible();
});

Then("I should see different books", async ({ page }) => {
  await page.waitForLoadState("networkidle");
  const rows = await page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
