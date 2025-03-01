---
dictionary:
  - id: Book
    name: Book
    summary: "A written or printed work consisting of pages glued or sewn together, available in the BookWorm catalog."
    description: |
      A book represents a single publication in the catalog. Key attributes include:

      - Title and subtitle
      - Author(s)
      - Publisher
      - ISBN (International Standard Book Number)
      - Publication date
      - Edition and format (e.g., hardcover, paperback, eBook)
      - Description and synopsis
      - Cover image
      - Price and availability

      Books are the core entity in the catalog and are organized by categories, authors, and publishers.
    icon: BookOpen

  - id: Publisher
    name: Publisher
    summary: "An entity responsible for producing and distributing books."
    description: |
      Publishers are key partners in the BookWorm ecosystem. They provide books to the catalog and are associated with:

      - Publisher name and contact information
      - List of published books
      - Publishing history and reputation
      - Licensing and distribution agreements

      Publishers help ensure a diverse and high-quality catalog of books.
    icon: Printer

  - id: Category
    name: Category
    summary: "A classification used to organize books into groups based on genre, topic, or other criteria."
    description: |
      Categories help customers browse and discover books more easily. Key aspects include:

      - Category name (e.g., Fiction, Non-Fiction, Science Fiction, Biography)
      - Hierarchical structure (e.g., parent and subcategories)
      - List of books belonging to the category
      - Metadata for filtering and searching (e.g., age group, reading level)

      Categories improve the user experience by enabling targeted browsing and recommendations.
    icon: Folder

  - id: Author
    name: Author
    summary: "The creator or writer of a book."
    description: |
      Authors are central to the catalog, as they are the source of the content. Key details include:

      - Author name and biography
      - List of books written by the author
      - Awards and accolades
      - Social media or website links
      - Collaborations with other authors or publishers

      Authors help build the identity and appeal of the catalog.
    icon: User

  - id: ISBN
    name: ISBN
    summary: "International Standard Book Number - A unique identifier for books."
    description: |
      The ISBN is a critical identifier for books, ensuring uniqueness and traceability. It includes:

      - A 10- or 13-digit code
      - Publisher and title information encoded within the number
      - Used for inventory management, ordering, and sales tracking

      ISBNs are essential for managing large catalogs and ensuring accurate book identification.
    icon: Hash

  - id: Catalog
    name: Catalog
    summary: "The complete collection of books available in the BookWorm system."
    description: |
      The catalog is the central repository of all books, organized for browsing and discovery. It includes:

      - Search and filtering capabilities
      - Categorization by genre, author, and publisher
      - Metadata for each book (e.g., price, availability, ratings)
      - Integration with inventory and fulfillment systems

      The catalog is the primary interface for customers to explore and select books.
    icon: Library

  - id: Catalog Search
    name: Catalog Search
    summary: "The functionality allowing users to find books by keywords, titles, authors, or other criteria."
    description: |
      Catalog search enables users to quickly locate books in the catalog. Features include:

      - Full-text search across titles, authors, and descriptions
      - Filters by category, price range, format, and availability
      - Sorting options (e.g., by relevance, price, publication date)
      - Autocomplete and suggestions

      A robust search system enhances the user experience and drives sales.
    icon: Search

  - id: Catalog Metadata
    name: Catalog Metadata
    summary: "Descriptive information about books used for organization, search, and discovery."
    description: |
      Metadata enriches the catalog by providing additional context about books. It includes:

      - Tags and keywords
      - Ratings and reviews
      - Related books (e.g., by the same author or in the same category)
      - Reading level or age group
      - Awards and bestseller status

      Metadata improves discoverability and helps customers make informed decisions.
    icon: FileText

  - id: Catalog Update
    name: Catalog Update
    summary: "The process of adding, modifying, or removing books from the catalog."
    description: |
      Catalog updates ensure the catalog remains current and accurate. It involves:

      - Adding new books and their metadata
      - Updating existing book details (e.g., price, availability)
      - Removing discontinued or out-of-stock books
      - Syncing with publisher feeds or inventory systems

      Regular updates keep the catalog relevant and engaging for customers.
    icon: RefreshCw

  - id: Catalog Recommendation
    name: Catalog Recommendation
    summary: "A system for suggesting books to users based on their preferences or behavior."
    description: |
      Recommendations enhance the user experience by surfacing relevant books. They are based on:

      - User browsing and purchase history
      - Popular or trending books
      - Similar authors or categories
      - Collaborative filtering algorithms

      Recommendations drive engagement and increase sales.
    icon: ThumbsUp
---
