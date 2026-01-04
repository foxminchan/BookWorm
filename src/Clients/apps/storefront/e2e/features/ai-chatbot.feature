@high-priority @ai-features @chatbot
Feature: AI-Powered Book Recommendations
  As a customer
  I want to interact with an AI chatbot
  So that I can get personalized book recommendations and assistance

  Background:
    Given the storefront application is running
    And AI features are enabled

  @smoke @chat
  Scenario: Open AI chatbot from homepage
    Given I am on the homepage
    When I click "Try AI Recommendations" button
    Then the AI chatbot dialog should open
    And the chatbot should be focused
    And I should see a welcome message

  @smoke @chat
  Scenario: Open AI chatbot from floating button
    Given I am on any page
    When I click the floating chat button
    Then the AI chatbot dialog should open
    And the chatbot should be ready to receive messages

  @chat-interaction
  Scenario: Get book recommendation based on mood
    Given the AI chatbot is open
    When I type "I want something uplifting and inspiring"
    And I send the message
    Then I should receive AI-generated book recommendations
    And the recommendations should include book titles
    And each recommendation should have a brief description

  @chat-interaction
  Scenario: Ask for books similar to a specific title
    Given the AI chatbot is open
    When I type "I loved 'The Design of Everyday Things', what else would I enjoy?"
    And I send the message
    Then I should receive relevant book suggestions
    And the response should explain why the books are similar

  @chat-interaction
  Scenario: Search for books by topic
    Given the AI chatbot is open
    When I type "Can you recommend books about sustainable design?"
    And I send the message
    Then I should receive topic-specific recommendations
    And I should be able to click on book titles to view details

  @chat-navigation
  Scenario: Navigate to book from chatbot recommendation
    Given the AI chatbot is open
    And I received book recommendations
    When I click on a recommended book title
    Then I should be redirected to that book's detail page
    And the chatbot should remain available

  @chat-history
  Scenario: View conversation history
    Given the AI chatbot is open
    And I have sent multiple messages
    When I scroll up in the chat
    Then I should see my previous messages
    And I should see all AI responses
    And the conversation should be in chronological order

  @chat-clear
  Scenario: Clear chat history
    Given the AI chatbot is open
    And I have an existing conversation
    When I click "Clear Chat"
    Then the chat history should be cleared
    And I should see a fresh welcome message

  @accessibility @keyboard-nav
  Scenario: Navigate chatbot with keyboard
    Given I am on the homepage
    When I press Tab until the chat button is focused
    And I press Enter
    Then the chatbot dialog should open
    When I type a message
    And I press Tab to the send button
    And I press Enter
    Then the message should be sent

  @accessibility @focus-trap
  Scenario: Chatbot dialog traps focus
    Given the AI chatbot is open
    When I press Tab repeatedly
    Then focus should cycle within the dialog
    And focus should not escape to the page behind

  @chat-close
  Scenario: Close chatbot with escape key
    Given the AI chatbot is open
    When I press Escape
    Then the chatbot should close
    And focus should return to the trigger button

  @chat-close
  Scenario: Close chatbot with close button
    Given the AI chatbot is open
    When I click the close button
    Then the chatbot should close smoothly
    And my conversation should be preserved

  @error-handling
  Scenario: Handle AI service unavailability
    Given the AI chatbot is open
    And the AI service is unavailable
    When I send a message
    Then I should see an error notification
    And I should be prompted to try again later

  @rate-limiting
  Scenario: Handle rate limiting gracefully
    Given the AI chatbot is open
    And I have reached the rate limit
    When I send another message
    Then I should see a rate limit message
    And I should be informed when I can send again

  @mobile-experience
  Scenario: Chatbot is responsive on mobile
    Given I am on a mobile device
    And I am on the homepage
    When I click the floating chat button
    Then the chatbot should open full-screen
    And the input field should be accessible above the keyboard

  @smart-suggestions
  Scenario: Chatbot provides contextual suggestions
    Given the AI chatbot is open
    When I view the initial state
    Then I should see suggested prompts
    And I can click a suggestion to auto-fill the input

  @copy-response
  Scenario: Copy AI response to clipboard
    Given the AI chatbot is open
    And I received a book recommendation
    When I click the copy button on the response
    Then the response should be copied to clipboard
    And I should see a "Copied!" confirmation

  @attachment
  Scenario: Upload book cover for identification
    Given the AI chatbot is open
    When I click the attachment button
    And I upload a book cover image
    Then the AI should analyze the image
    And provide information about the book or similar titles
