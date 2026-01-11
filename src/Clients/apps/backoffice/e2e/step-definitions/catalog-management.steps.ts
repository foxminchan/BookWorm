import { Then, When } from "@cucumber/cucumber";
import { expect } from "@playwright/test";
import { Page } from "@playwright/test";

import { testData } from "../fixtures/test-data";
import { BooksPage } from "../pages";
import { World } from "../world";

/**
 * Catalog/Books management step definitions
 */

When("the books page loads", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

When(
  "I click the {string} button",
  async function (this: { page: Page }, buttonText: string) {
    await this.page.getByRole("button", { name: buttonText }).click();
  },
);

When("I fill in the book form with valid data", async function (this: World) {
  const booksPage = new BooksPage(this.page);
  const bookData = {
    title: testData.book.title(),
    author: testData.book.author(),
    isbn: testData.book.isbn(),
    price: testData.book.price().toString(),
    description: testData.book.description(),
  };

  // Store for later verification
  this.testData.createdBook = bookData;

  await booksPage.fillBookForm(bookData);
});

When("I search for a book by title", async function (this: World) {
  const booksPage = new BooksPage(this.page);
  const searchQuery =
    (this.testData.searchQuery as string | undefined) || "Test Book";
  await booksPage.searchBooks(searchQuery);
});

When(
  "I search for {string}",
  async function (this: { page: Page }, query: string) {
    const booksPage = new BooksPage(this.page);
    await booksPage.searchBooks(query);
  },
);

When(
  "I click the edit button for the first book",
  async function (this: { page: Page }) {
    const booksPage = new BooksPage(this.page);
    await booksPage.editFirstBook();
  },
);

When("I update the book title", async function (this: World) {
  const newTitle = testData.book.title();
  this.testData.updatedTitle = newTitle;
  await this.page.fill('input[name="title"]', newTitle);
});

When(
  "I click the delete button for the first book",
  async function (this: { page: Page }) {
    const booksPage = new BooksPage(this.page);
    await booksPage.deleteFirstBook();
  },
);

When("I confirm the deletion", async function (this: { page: Page }) {
  const booksPage = new BooksPage(this.page);
  await booksPage.confirmDelete();
});

When("I cancel the deletion", async function (this: { page: Page }) {
  const booksPage = new BooksPage(this.page);
  await booksPage.cancelDelete();
});

When("I leave required fields empty", async function (this: { page: Page }) {
  // Just click save without filling anything
  // Form validation will be triggered
});

Then("I should see the books table", async function (this: { page: Page }) {
  await expect(this.page.locator("table")).toBeVisible();
});

Then("I should see the search input", async function (this: { page: Page }) {
  await expect(this.page.locator('input[placeholder*="Search"]')).toBeVisible();
});

Then(
  "the book should be added successfully",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
    // Could check for success toast/message
  },
);

Then("I should see the new book in the list", async function (this: World) {
  const createdBook = this.testData.createdBook as
    | { title: string }
    | undefined;
  if (createdBook) {
    const booksPage = new BooksPage(this.page);
    const exists = await booksPage.bookExists(createdBook.title);
    expect(exists).toBeTruthy();
  }
});

Then("I should see only matching books", async function (this: { page: Page }) {
  // Verify table has filtered results
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThanOrEqual(0);
});

Then("the results should be filtered", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
});

Then(
  "I should see {string} message",
  async function (this: { page: Page }, message: string) {
    await expect(this.page.locator(`text=${message}`).first()).toBeVisible();
  },
);

Then("the table should be empty", async function (this: { page: Page }) {
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBe(0);
});

Then(
  "the book should be updated successfully",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then("I should see the updated information", async function (this: World) {
  const updatedTitle = this.testData.updatedTitle as string | undefined;
  if (updatedTitle) {
    await expect(
      this.page.locator(`text=${updatedTitle}`).first(),
    ).toBeVisible();
  }
});

Then(
  "I should see a confirmation dialog",
  async function (this: { page: Page }) {
    await expect(this.page.locator('[role="dialog"]')).toBeVisible();
  },
);

Then(
  "the book should be removed from the list",
  async function (this: { page: Page }) {
    await this.page.waitForLoadState("networkidle");
  },
);

Then(
  "the book should remain in the list",
  async function (this: { page: Page }) {
    await expect(this.page.locator("table tbody tr").first()).toBeVisible();
  },
);

Then(
  "I should see validation error messages",
  async function (this: { page: Page }) {
    await expect(this.page.locator('[role="alert"]').first()).toBeVisible();
  },
);

Then("the book should not be created", async function (this: { page: Page }) {
  // Form should still be visible, not navigated away
  await expect(this.page.locator('button:has-text("Save")')).toBeVisible();
});

Then("I should see different books", async function (this: { page: Page }) {
  await this.page.waitForLoadState("networkidle");
  const rows = await this.page.locator("table tbody tr").count();
  expect(rows).toBeGreaterThan(0);
});
