import { Page } from "@playwright/test";

import { BasePage } from "./BasePage";

/**
 * Books Management Page Object
 * Handles /backoffice/books, /backoffice/books/new, and /backoffice/books/[id]
 */
export class BooksPage extends BasePage {
  private readonly pageHeading = 'h1:has-text("Books Management")';
  private readonly addBookLink = 'a:has-text("Add New Book")';
  private readonly expandFiltersButton = 'button:has-text("Expand filters")';
  private readonly searchInput = "#book-search";
  private readonly booksTable = "table";
  private readonly tableRows = "table tbody tr";
  private readonly editLinks = 'a[aria-label^="Edit "]';
  private readonly deleteButtons = 'button[aria-label^="Delete "]';
  private readonly confirmDialog = '[role="alertdialog"]';
  private readonly confirmDeleteButton =
    '[role="alertdialog"] button:has-text("Delete")';
  private readonly cancelDialogButton =
    '[role="alertdialog"] button:has-text("Cancel")';

  // Form selectors (book-form.tsx, book-info-card.tsx, classification-card.tsx)
  private readonly nameInput = 'input[name="name"]';
  private readonly descriptionInput = 'textarea[name="description"]';
  private readonly priceInput = 'input[name="price"]';
  private readonly authorCheckboxes = 'button[role="checkbox"][id^="author-"]';
  private readonly submitButton = 'button[type="submit"]';

  constructor(page: Page) {
    super(page);
  }

  /**
   * Navigate to books page
   */
  async navigate(): Promise<void> {
    await this.goto("/books");
  }

  /**
   * Assert books page is loaded
   */
  async assertIsBooksPage(): Promise<void> {
    await this.assertVisible(this.pageHeading);
    await this.assertVisible(this.booksTable);
  }

  /**
   * Click Add New Book link
   */
  async clickAddBook(): Promise<void> {
    await this.click(this.addBookLink);
    await this.waitForNavigation();
  }

  /**
   * Expand the collapsible filter panel if collapsed
   */
  async expandFilters(): Promise<void> {
    const searchVisible = await this.isVisible(this.searchInput);
    if (!searchVisible) {
      await this.click(this.expandFiltersButton);
      await this.waitForElement(this.searchInput);
    }
  }

  /**
   * Search for books (expands filters if needed, then types into debounced search input)
   */
  async searchBooks(query: string): Promise<void> {
    await this.expandFilters();
    await this.fill(this.searchInput, query);
    // BooksFilters debounces search by 500ms
    await this.page.waitForTimeout(600);
    await this.waitForPageLoad();
  }

  /**
   * Get number of book data rows in table (excludes "No data found" empty state row)
   */
  async getBooksCount(): Promise<number> {
    const emptyCell = this.page.locator(
      'table tbody td:has-text("No data found")',
    );
    if (await emptyCell.isVisible()) {
      return 0;
    }
    return await this.page.locator(this.tableRows).count();
  }

  /**
   * Fill book form with valid data (selects first available category, publisher, and author)
   */
  async fillBookForm(bookData: {
    name: string;
    price: string;
    description: string;
  }): Promise<void> {
    await this.fill(this.nameInput, bookData.name);
    await this.fill(this.descriptionInput, bookData.description);
    await this.fill(this.priceInput, bookData.price);

    // Select first category
    await this.page.getByLabel("Category").click();
    await this.page.locator('[role="option"]').first().click();

    // Select first publisher
    await this.page.getByLabel("Publisher").click();
    await this.page.locator('[role="option"]').first().click();

    // Select first author checkbox
    const firstAuthor = this.page.locator(this.authorCheckboxes).first();
    if ((await firstAuthor.getAttribute("data-state")) !== "checked") {
      await firstAuthor.click();
    }
  }

  /**
   * Submit book form (Create Book / Update Book)
   */
  async submitBookForm(): Promise<void> {
    await this.click(this.submitButton);
    await this.waitForNavigation();
  }

  /**
   * Edit first book in table
   */
  async editFirstBook(): Promise<void> {
    await this.page.locator(this.editLinks).first().click();
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
    await this.click(this.confirmDeleteButton);
    await this.waitForPageLoad();
  }

  /**
   * Cancel deletion dialog
   */
  async cancelDelete(): Promise<void> {
    await this.click(this.cancelDialogButton);
  }

  /**
   * Check if book exists in table by title
   */
  async bookExists(title: string): Promise<boolean> {
    const cell = this.page.locator(`td:has-text("${title}")`).first();
    return await cell.isVisible();
  }
}
