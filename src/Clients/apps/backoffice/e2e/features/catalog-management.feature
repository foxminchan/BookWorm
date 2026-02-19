@catalog
@books
@p1
Feature: Book Catalog Management
  As an admin
  I want to manage the book catalog
  So that customers can browse and purchase books

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the books page

  @smoke
  @view-books
  Scenario: View list of books
    When the books page loads
    Then I should see the books table
    And I should see the "Add Book" button
    And I should see the search input

  @create-book
  Scenario: Add a new book successfully
    When I click the "Add Book" button
    And I fill in the book form with valid data
    And I click the "Save" button
    Then the book should be added successfully
    And I should see the new book in the list

  @search
  @books
  Scenario: Search for books by title
    Given the catalog has multiple books
    When I search for a book by title
    Then I should see only matching books
    And the results should be filtered

  @search
  @books
  Scenario: Search with no results shows empty state
    When I search for "NonExistentBook12345"
    Then I should see "No books found" message
    And the table should be empty

  @update-book
  Scenario: Edit an existing book
    Given the catalog has at least one book
    When I click the edit button for the first book
    And I update the book title
    And I click the "Save" button
    Then the book should be updated successfully
    And I should see the updated information

  @delete-book
  Scenario: Delete a book with confirmation
    Given the catalog has at least one book
    When I click the delete button for the first book
    Then I should see a confirmation dialog
    When I confirm the deletion
    Then the book should be removed from the list

  @delete-book
  @cancel
  Scenario: Cancel book deletion
    Given the catalog has at least one book
    When I click the delete button for the first book
    And I cancel the deletion
    Then the book should remain in the list

  @validation
  @create-book
  Scenario: Create book with missing required fields shows validation errors
    When I click the "Add Book" button
    And I leave required fields empty
    And I click the "Save" button
    Then I should see validation error messages
    And the book should not be created

  @pagination
  Scenario: Navigate through book pages
    Given the catalog has more than 10 books
    When I navigate to page 2
    Then I should see different books
    And the pagination should update
