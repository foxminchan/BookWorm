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
    And I should see the "Add New Book" button
    And I should see the filter toggle button

  @create-book
  Scenario: Add a new book successfully
    When I click the "Add New Book" button
    And I fill in the book form with valid data
    And I click the "Create Book" button
    Then the book should be added successfully
    And I should see the new book in the list

  @search
  @books
  Scenario: Search for books by title or author
    Given the catalog has multiple books
    When I expand the book filters
    And I search for a book by title
    Then I should see only matching books
    And the results should be filtered

  @search
  @books
  Scenario: Search with no results shows empty state
    When I expand the book filters
    And I search for "NonExistentBook12345"
    Then I should see "No data found" message
    And the books table should be empty

  @update-book
  Scenario: Edit an existing book
    Given the catalog has at least one book
    When I click the edit button for the first book
    And I update the book name
    And I click the "Update Book" button
    Then the book should be updated successfully
    And I should see the updated book in the list

  @delete-book
  Scenario: Delete a book with confirmation
    Given the catalog has at least one book
    When I click the delete button for the first book
    Then I should see a confirmation dialog
    When I confirm the book deletion
    Then the book should be removed from the list
    And I should see a success message

  @delete-book
  @cancel
  Scenario: Cancel book deletion
    Given the catalog has at least one book
    When I click the delete button for the first book
    And I cancel the book deletion
    Then the book should remain in the list

  @validation
  @create-book
  Scenario: Create book without required classification keeps submit disabled
    When I click the "Add New Book" button
    And I leave required fields empty
    Then I should see the missing author validation message
    And the "Create Book" button should be disabled

  @pagination
  Scenario: Navigate through book pages
    Given the catalog has more than 10 books
    When I navigate to the next page
    Then I should see different books
    And the pagination should update
