@high-priority
@catalog
@p1
Feature: Product Search and Filtering
  As a customer
  I want to search and filter books
  So that I can find specific books easily

  Background:
    Given the storefront application is running
    And the catalog has at least 25 books available

  @search
  @smoke
  Scenario: Search by keyword
    Given I am on the shop page
    When I enter "design" in the search bar
    Then I should see books matching "design" in the results
    And the URL should contain "search=design"

  @filters
  Scenario: Apply multiple filters simultaneously
    Given I am on the shop page
    When I enter "programming" in the search bar
    And I select "Technology" category filter
    And I set price range to "$20-$50"
    Then the results should be filtered accordingly
    And the URL should contain "search=programming"
    And the URL should contain "category=technology"

  @sort
  Scenario: Sort books by price ascending
    Given I am on the shop page
    When I sort by "Price: Low to High"
    Then books should be displayed in ascending price order

  @sort
  Scenario: Sort books by price descending
    Given I am on the shop page
    When I sort by "Price: High to Low"
    Then books should be displayed in descending price order

  @pagination
  @smoke
  Scenario: Navigate through multiple pages of results
    Given I am on the shop page with 25 books available
    Then I should see 8 books on page 1

    When I click "Next Page"
    Then I should see page 2 in the URL
    And I should see 8 more books

    When I click page 3
    Then I should see page 3 in the URL
    And I should see the remaining 9 books

  @pagination
  Scenario: Pagination preserves filters
    Given I am on the shop page
    When I select "Fiction" category filter
    And I click "Next Page"
    Then the URL should contain "category=fiction"
    And the URL should contain "page=2"
    And only "Fiction" books should be displayed

  @filters
  Scenario: Single category filter selection
    Given I am on the shop page
    When I select "Art & Design" category filter
    Then only "Art & Design" books should be displayed
    And the "Art & Design" filter should be checked

  @filters
  Scenario: Single publisher filter selection
    Given I am on the shop page
    When I select "Prentice Hall" publisher filter
    Then only books from "Prentice Hall" should be displayed
    And the "Prentice Hall" filter should be checked

  @filters
  Scenario: Clear all filters
    Given I am on the shop page
    And I have applied category and price filters
    When I click "Clear Filters"
    Then all filters should be reset
    And I should see all available books

  @empty-state
  Scenario: No results found shows empty state
    Given I am on the shop page
    When I enter "xyzabc123nonexistent" in the search bar
    Then I should see "No books found" message
    And I should see "Clear Filters" button

  @categories
  Scenario: Browse books by category from homepage
    Given I am on the homepage
    When I click on "Fiction" category
    Then I should be redirected to shop page
    And only "Fiction" books should be displayed
    And the "Fiction" filter should be selected

  @categories
  Scenario: View all categories
    Given I am on the homepage
    When I click "Categories"
    Then I should see all available categories
    And I should see at least 6 categories

  @publishers
  Scenario: View all publishers
    Given I am on the homepage
    When I click "Publishers"
    Then I should see all available publishers
    And I should see at least 4 publishers
