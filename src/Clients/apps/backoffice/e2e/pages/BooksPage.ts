import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Books Management Page Object
 * Handles catalog/books CRUD operations
 */
export class BooksPage extends BasePage {
  // Selectors
  private readonly pageHeading = 'h1:has-text("Books")';
  private readonly addBookButton = 'button:has-text("Add Book")';
  private readonly searchInput = 'input[placeholder*="Search"]';
  private readonly booksTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly editButtons = 'button:has-text("Edit")';
  private readonly deleteButtons = 'button:has-text("Delete")';
  private readonly confirmDialog = '[role="dialog"]';
  private readonly confirmButton = 'button:has-text("Confirm")';
  private readonly cancelButton = 'button:has-text("Cancel")';

  // Form selectors
  private readonly titleInput = 'input[name="title"]';
  private readonly authorInput = 'input[name="author"]';
  private readonly isbnInput = 'input[name="isbn"]';
  private readonly priceInput = 'input[name="price"]';
  private readonly descriptionInput = 'textarea[name="description"]';
  private readonly saveButton = 'button:has-text("Save")';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to books page
   */
  async navigate(): Promise<void> {
    await this.goto("/catalog/books");
  }

  /**
   * Assert books page is loaded
   */
  async assertIsBooksPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.booksTable);
  }

  /**
   * Click add book button
   */
  async clickAddBook(): Promise<void> {
    await this.click(this.addBookButton);
  }

  /**
   * Search for books
   */
  async searchBooks(query: string): Promise<void> {
    await this.fill(this.searchInput, query);
    await this.pressKey("Enter");
    await this.waitForPageLoad();
  }

  /**
   * Get number of books in table
   */
  async getBooksCount(): Promise<number> {
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * Fill book form
   */
  async fillBookForm(bookData: {
    title: string;
    author: string;
    isbn: string;
    price: string;
    description: string;
  }): Promise<void> {
    await this.fill(this.titleInput, bookData.title);
    await this.fill(this.authorInput, bookData.author);
    await this.fill(this.isbnInput, bookData.isbn);
    await this.fill(this.priceInput, bookData.price);
    await this.fill(this.descriptionInput, bookData.description);
  }

  /**
   * Save book form
   */
  async saveBook(): Promise<void> {
    await this.click(this.saveButton);
    await this.waitForNavigation();
  }

  /**
   * Edit first book in table
   */
  async editFirstBook(): Promise<void> {
    await this.page.locator(this.editButtons).first().click();
    await this.waitForPageLoad();
  }

  /**
   * Delete first book in table
   */
  async deleteFirstBook(): Promise<void> {
    await this.page.locator(this.deleteButtons).first().click();
    await this.waitForElement(this.confirmDialog);
  }

  /**
   * Confirm deletion dialog
   */
  async confirmDelete(): Promise<void> {
    await this.click(this.confirmButton);
    await this.waitForPageLoad();
  }

  /**
   * Cancel deletion dialog
   */
  async cancelDelete(): Promise<void> {
    await this.click(this.cancelButton);
  }

  /**
   * Check if book exists in table by title
   */
  async bookExists(title: string): Promise<boolean> {
    const cell = this.page.locator(`td:has-text("${title}")`);
    return await cell.isVisible();
  }
}
