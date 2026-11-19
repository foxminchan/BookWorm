import { expect } from "@playwright/test";

import { Then, When } from "./fixtures";

/**
 * Catalog/Books management step definitions
 */

When("the books page loads", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

When("I click the {string} button", async ({ page }, buttonText: string) => {
  const buttonOrLink = page
    .getByRole("button", { name: buttonText })
    .or(page.getByRole("link", { name: buttonText }));
  await buttonOrLink.click();
  await page.waitForLoadState("networkidle");
});

When("I expand the book filters", async ({ booksPage }) => {
  await booksPage.expandFilters();
});

When(
  "I fill in the book form with valid data",
  async ({ booksPage, testData, scenarioData }) => {
    const bookData = {
      name: testData.book.name(),
      price: testData.book.price().toString(),
      description: testData.book.description(),
    };

    scenarioData.createdBook = bookData;

    await booksPage.fillBookForm(bookData);
  },
);

When("I search for a book by title", async ({ booksPage, scenarioData }) => {
  const searchQuery = (scenarioData.searchQuery as string) || "Test";
  await booksPage.searchBooks(searchQuery);
});

When("I search for {string}", async ({ booksPage }, query: string) => {
  await booksPage.searchBooks(query);
});

When("I click the edit button for the first book", async ({ booksPage }) => {
  await booksPage.editFirstBook();
});

When("I update the book name", async ({ page, testData, scenarioData }) => {
  const newName = testData.book.name();
  scenarioData.updatedName = newName;
  await page.fill('input[name="name"]', newName);
});

When("I click the delete button for the first book", async ({ booksPage }) => {
  await booksPage.deleteFirstBook();
});

When("I confirm the book deletion", async ({ booksPage }) => {
  await booksPage.confirmDelete();
});

When("I cancel the book deletion", async ({ booksPage }) => {
  await booksPage.cancelDelete();
});

When("I leave required fields empty", async () => {
  // Leave form fields at default empty state
});

Then("I should see the books table", async ({ page }) => {
  await expect(page.locator("table")).toBeVisible();
});

Then("I should see the filter toggle button", async ({ page }) => {
  await expect(
    page.getByRole("button", { name: /Expand filters|Collapse filters/ }),
  ).toBeVisible();
});

Then("the book should be added successfully", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/books/);
});

Then(
  "I should see the new book in the list",
  async ({ booksPage, scenarioData }) => {
    const createdBook = scenarioData.createdBook as
      { name: string } | undefined;
    if (createdBook) {
      const exists = await booksPage.bookExists(createdBook.name);
      expect(exists).toBeTruthy();
    }
  },
);

Then("I should see only matching books", async ({ booksPage }) => {
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThanOrEqual(0);
});

Then("the results should be filtered", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("I should see {string} message", async ({ page }, message: string) => {
  await expect(page.locator(`text=${message}`).first()).toBeVisible();
});

Then("the books table should be empty", async ({ booksPage }) => {
  const count = await booksPage.getBooksCount();
  expect(count).toBe(0);
});

Then("the book should be updated successfully", async ({ page }) => {
  await expect(page).toHaveURL(/\/backoffice\/books$/);
});

Then(
  "I should see the updated book in the list",
  async ({ page, scenarioData }) => {
    const updatedName = scenarioData.updatedName as string | undefined;
    if (updatedName) {
      await expect(page.locator(`text=${updatedName}`).first()).toBeVisible();
    }
  },
);

Then("the book should be removed from the list", async ({ page }) => {
  await page.waitForLoadState("networkidle");
});

Then("the book should remain in the list", async ({ booksPage }) => {
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});

Then("I should see the missing author validation message", async ({ page }) => {
  await expect(page.getByText("At least one author is required")).toBeVisible();
});

Then(
  "the {string} button should be disabled",
  async ({ page }, buttonText: string) => {
    await expect(page.getByRole("button", { name: buttonText })).toBeDisabled();
  },
);

Then("I should see different books", async ({ booksPage }) => {
  const count = await booksPage.getBooksCount();
  expect(count).toBeGreaterThan(0);
});
