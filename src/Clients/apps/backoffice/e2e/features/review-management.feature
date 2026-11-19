@rating
@reviews
@p2
Feature: Review Management
  As an admin
  I want to manage customer reviews
  So that I can maintain quality and appropriate content

  Background:
    Given the backoffice application is running
    And I am logged in as an admin
    And I am on the reviews page

  @smoke
  @view-reviews
  Scenario: Reviews page prompts to select a book initially
    When the reviews page loads
    Then I should see the book selector
    And I should see "Select a book above to view and manage its reviews"

  @smoke
  @view-reviews
  Scenario: View list of reviews for a selected book
    When I select the first book in the review selector
    Then I should see the reviews table
    And I should see review Customer, Rating, and Comment columns

  @delete-review
  Scenario: Delete a review with confirmation
    Given I have selected a book with at least one review
    When I click the delete button for the first review
    Then I should see a confirmation dialog
    When I confirm the review deletion
    Then I should see a success message

  @delete-review
  @cancel
  Scenario: Cancel review deletion
    Given I have selected a book with at least one review
    When I click the delete button for the first review
    And I cancel the review deletion
    Then the review should remain in the list

  @pagination
  @reviews
  Scenario: Navigate through review pages
    Given I have selected a book with more than 10 reviews
    When I navigate to the next page
    Then I should see different reviews
    And the pagination should update
