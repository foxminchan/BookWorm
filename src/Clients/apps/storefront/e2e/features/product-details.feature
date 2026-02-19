@high-priority
@product-details
@reviews
@p1
Feature: Product Detail Page and Reviews
  As a customer
  I want to view detailed book information and reviews
  So that I can make informed purchase decisions

  Background:
    Given the storefront application is running
    And the catalog has available books

  @smoke
  Scenario: View complete product information
    Given I am on a product detail page for "The Design of Everyday Things"
    Then I should see the book title
    And I should see the book price
    And I should see the publisher information
    And I should see the category
    And I should see the authors
    And I should see the rating
    And I should see the product image

  @add-to-basket
  Scenario: Add product to basket from detail page
    Given I am on a product detail page
    When I click "Add to Basket"
    Then the book should be added to my basket
    And the basket count should increase by 1

  @quantity
  Scenario: Add product with custom quantity
    Given I am on a product detail page
    When I increase quantity to 5
    And I click "Add to Basket"
    Then the basket count should show 5 items

  @sale-price
  Scenario: Display sale price when applicable
    Given I am on a product detail page for a book on sale
    Then I should see the original price
    And I should see the sale price
    And I should see a "Sale" badge

  @reviews
  @smoke
  Scenario: Submit a product review
    Given I am on a product detail page
    When I scroll to the reviews section
    And I click "Write a Review"
    Then I should see the review form

    When I select 5 stars
    And I enter "John" in first name
    And I enter "Doe" in last name
    And I enter "Excellent book with great insights!" in comment
    And I click "Submit Review"
    Then I should see a success message
    And my review should appear in the reviews list

  @reviews
  Scenario: View and paginate reviews
    Given I am on a product detail page with 12 reviews
    Then I should see 5 reviews on the first page

    When I click page 2 of reviews
    Then I should see the next 5 reviews

  @reviews
  @sort
  Scenario: Sort reviews by highest rating
    Given I am on a product detail page with multiple reviews
    When I scroll to the reviews section
    And I select "Highest Rating" sort
    Then reviews should be ordered by rating from highest to lowest

  @reviews
  @sort
  Scenario: Sort reviews by lowest rating
    Given I am on a product detail page with multiple reviews
    When I scroll to the reviews section
    And I select "Lowest Rating" sort
    Then reviews should be ordered by rating from lowest to highest

  @reviews
  @sort
  Scenario: Sort reviews by newest first
    Given I am on a product detail page with multiple reviews
    When I scroll to the reviews section
    And I select "Newest" sort
    Then reviews should be ordered by date from newest to oldest

  @reviews
  @ai
  Scenario: Generate AI summary of reviews
    Given I am on a product detail page with multiple reviews
    When I scroll to the reviews section
    And I click "Generate Summary"
    Then I should see an AI-generated summary of reviews

  @stock
  Scenario: In-stock product allows add to basket
    Given I am on a product detail page for an in-stock book
    Then I should see "In Stock" status
    And the "Add to Basket" button should be enabled

  @stock
  @edge-case
  Scenario: Out-of-stock product disables add to basket
    Given I am on a product detail page for an out-of-stock book
    Then I should see "Out of Stock" status
    And the "Add to Basket" button should be disabled

  @validation
  Scenario: Review form validation
    Given I am on a product detail page
    When I click "Write a Review"
    And I click "Submit Review" without filling the form
    Then I should see validation errors

  @reviews
  @empty-state
  Scenario: Product with no reviews shows empty state
    Given I am on a product detail page with no reviews
    When I scroll to the reviews section
    Then I should see "No reviews yet" message
    And I should see "Be the first to review" button
