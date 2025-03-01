---
dictionary:
  - id: Rating Service
    name: Rating Service
    summary: "A service that manages user ratings and reviews for books in the BookWorm catalog."
    description: |
      The Rating Service enables customers to share their feedback on books and helps others make informed decisions. Key responsibilities include:

      - Collecting and storing ratings and reviews
      - Calculating average ratings for books
      - Validating and moderating user-submitted content
      - Displaying ratings and reviews on book pages
      - Supporting upvoting and reporting features

      This service enhances the user experience by fostering community engagement and trust.
    icon: Star

  - id: Rating
    name: Rating
    summary: "A numerical score (e.g., 1 to 5 stars) given by a user to rate a book."
    description: |
      Ratings provide a quick way for users to express their satisfaction with a book. Key aspects include:

      - Scale: Typically 1 to 5 stars, with 5 being the highest
      - Weight: Contributes to the book's overall average rating
      - Timestamp: Date and time the rating was submitted
      - User ID: Identifier of the user who submitted the rating

      Ratings are aggregated to calculate the book's average score.
    icon: Star

  - id: Review
    name: Review
    summary: "A written evaluation of a book submitted by a user."
    description: |
      Reviews allow users to provide detailed feedback about their experience with a book. They include:

      - Title and body text
      - Rating (optional, but often paired with a review)
      - Timestamp and user ID
      - Helpfulness votes (e.g., "Was this review helpful?")
      - Moderation status (e.g., approved, flagged, removed)

      Reviews help other customers make informed decisions and provide valuable feedback to authors and publishers.
    icon: FileText

  - id: Average Rating
    name: Average Rating
    summary: "The aggregated score of all ratings for a book, displayed as an average."
    description: |
      The average rating provides a quick snapshot of a book's overall quality. It is calculated by:

      - Summing all individual ratings
      - Dividing by the total number of ratings
      - Rounding to a standard format (e.g., 4.3 out of 5)

      Average ratings are prominently displayed on book pages and in search results.
    icon: BarChart2

  - id: Helpfulness Vote
    name: Helpfulness Vote
    summary: "A user's indication of whether a review was helpful or not."
    description: |
      Helpfulness votes allow users to rate the quality of reviews. Features include:

      - Upvoting or downvoting a review
      - Displaying the net helpfulness score (e.g., "10 out of 12 users found this helpful")
      - Sorting reviews by helpfulness

      This feature ensures that the most useful reviews rise to the top.
    icon: ThumbsUp

  - id: Review Moderation
    name: Review Moderation
    summary: "The process of validating and filtering user-submitted reviews to ensure quality and appropriateness."
    description: |
      Review moderation maintains the integrity of the Rating Service. It involves:

      - Automated filtering for inappropriate language
      - Manual review of flagged content
      - Approving, editing, or removing reviews
      - Notifying users of moderation actions

      Moderation ensures that reviews are respectful and relevant.
    icon: Shield

  - id: Review Flagging
    name: Review Flagging
    summary: "The process of reporting inappropriate or irrelevant reviews for moderation."
    description: |
      Review flagging allows users to report problematic content. Key aspects include:

      - Reasons for flagging (e.g., offensive language, spoilers, irrelevant content)
      - User ID of the flagger
      - Timestamp of the flag
      - Moderation status (e.g., pending, resolved)

      Flagging helps maintain a high-quality review ecosystem.
    icon: Flag

  - id: Verified Purchase
    name: Verified Purchase
    summary: "A badge indicating that a review was submitted by a user who purchased the book on BookWorm."
    description: |
      Verified purchases add credibility to reviews. Key features include:

      - Displaying a "Verified Purchase" badge on reviews
      - Ensuring the reviewer bought the book through BookWorm
      - Encouraging more authentic and trustworthy feedback

      This feature helps users distinguish between verified and unverified reviews.
    icon: CheckCircle

  - id: Review Analytics
    name: Review Analytics
    summary: "Data and insights derived from user ratings and reviews."
    description: |
      Review analytics provide valuable information for authors, publishers, and the BookWorm team. They include:

      - Distribution of ratings (e.g., percentage of 5-star reviews)
      - Common themes in reviews (e.g., positive or negative feedback trends)
      - Helpfulness scores and engagement metrics
      - Impact of reviews on sales and customer satisfaction

      Analytics help improve the catalog and user experience.
    icon: PieChart

  - id: Review Notification
    name: Review Notification
    summary: "A notification sent to users to encourage them to submit a review after purchasing a book."
    description: |
      Review notifications help increase the number of reviews and improve engagement. They include:

      - Reminders to rate and review purchased books
      - Links to the review submission page
      - Incentives for submitting reviews (e.g., loyalty points)

      These notifications are sent via email or in-app messages.
    icon: Bell
---
