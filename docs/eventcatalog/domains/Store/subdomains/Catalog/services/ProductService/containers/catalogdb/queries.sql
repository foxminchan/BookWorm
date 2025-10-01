-- Business Intelligence Queries for Catalog Database
-- These queries provide valuable insights for business decision-making

-- ============================================
-- INVENTORY & PRODUCT MANAGEMENT
-- ============================================

-- 1. Top 10 Best-Selling Books by Average Rating
-- Purpose: Identify high-performing products for promotion and restocking
SELECT
    b.id,
    b.name,
    b.average_rating,
    b.total_reviews,
    b.price_original_price,
    b.price_discount_price,
    c.name AS category,
    p.name AS publisher,
    b.status
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = FALSE
    AND b.total_reviews > 10  -- Minimum reviews for statistical significance
ORDER BY b.average_rating DESC, b.total_reviews DESC
LIMIT 10;

-- 2. Books with Low Stock Status
-- Purpose: Identify products that need restocking
SELECT
    b.id,
    b.name,
    b.status,
    b.average_rating,
    b.total_reviews,
    p.name AS publisher
FROM books b
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = FALSE
    AND b.status != 0  -- Assuming 0 = InStock
ORDER BY b.total_reviews DESC;

-- 3. Books with Discounts and Their Performance
-- Purpose: Analyze effectiveness of pricing strategies
SELECT
    b.id,
    b.name,
    b.price_original_price,
    b.price_discount_price,
    ROUND(((b.price_original_price - b.price_discount_price) / b.price_original_price * 100)::numeric, 2) AS discount_percentage,
    b.average_rating,
    b.total_reviews,
    c.name AS category
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
WHERE b.is_deleted = FALSE
    AND b.price_discount_price IS NOT NULL
    AND b.price_discount_price < b.price_original_price
ORDER BY discount_percentage DESC;

-- 4. Books Without Reviews
-- Purpose: Identify products that need marketing attention or customer engagement
SELECT
    b.id,
    b.name,
    b.price_original_price,
    c.name AS category,
    p.name AS publisher,
    b.created_at
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = FALSE
    AND b.total_reviews = 0
ORDER BY b.created_at DESC;

-- ============================================
-- CATEGORY ANALYSIS
-- ============================================

-- 5. Category Performance Overview
-- Purpose: Understand which categories drive the most engagement
SELECT
    c.id,
    c.name AS category,
    COUNT(b.id) AS total_books,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_category_rating,
    SUM(b.total_reviews) AS total_reviews,
    ROUND(AVG(b.price_original_price)::numeric, 2) AS avg_price,
    COUNT(CASE WHEN b.price_discount_price IS NOT NULL THEN 1 END) AS books_on_sale
FROM categories c
LEFT JOIN books b ON c.id = b.category_id AND b.is_deleted = FALSE
GROUP BY c.id, c.name
ORDER BY total_books DESC, total_reviews DESC;

-- 6. Most Popular Categories by Review Count
-- Purpose: Identify trending categories for inventory planning
SELECT
    c.name AS category,
    COUNT(b.id) AS book_count,
    SUM(b.total_reviews) AS total_reviews,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating
FROM categories c
INNER JOIN books b ON c.id = b.category_id
WHERE b.is_deleted = FALSE
    AND b.total_reviews > 0
GROUP BY c.name
ORDER BY total_reviews DESC
LIMIT 10;

-- 7. Categories with Highest Revenue Potential
-- Purpose: Calculate potential revenue by category
SELECT
    c.name AS category,
    COUNT(b.id) AS total_books,
    ROUND(SUM(COALESCE(b.price_discount_price, b.price_original_price))::numeric, 2) AS total_inventory_value,
    ROUND(AVG(COALESCE(b.price_discount_price, b.price_original_price))::numeric, 2) AS avg_selling_price,
    SUM(b.total_reviews) AS customer_interest
FROM categories c
INNER JOIN books b ON c.id = b.category_id
WHERE b.is_deleted = FALSE
GROUP BY c.name
ORDER BY total_inventory_value DESC;

-- ============================================
-- AUTHOR & PUBLISHER INSIGHTS
-- ============================================

-- 8. Top Authors by Book Count and Average Rating
-- Purpose: Identify prolific and well-reviewed authors for partnerships
SELECT
    a.id,
    a.name AS author,
    COUNT(DISTINCT ba.book_id) AS books_count,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating,
    SUM(b.total_reviews) AS total_reviews
FROM authors a
INNER JOIN book_authors ba ON a.id = ba.author_id
INNER JOIN books b ON ba.book_id = b.id
WHERE b.is_deleted = FALSE
GROUP BY a.id, a.name
HAVING COUNT(DISTINCT ba.book_id) > 0
ORDER BY books_count DESC, avg_rating DESC
LIMIT 20;

-- 9. Publisher Performance Scorecard
-- Purpose: Evaluate publisher relationships and catalog quality
SELECT
    p.id,
    p.name AS publisher,
    COUNT(b.id) AS total_books,
    COUNT(CASE WHEN b.status = 0 THEN 1 END) AS in_stock_books,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating,
    SUM(b.total_reviews) AS total_reviews,
    ROUND(AVG(b.price_original_price)::numeric, 2) AS avg_price,
    COUNT(CASE WHEN b.price_discount_price IS NOT NULL THEN 1 END) AS discounted_books
FROM publishers p
LEFT JOIN books b ON p.id = b.publisher_id AND b.is_deleted = FALSE
GROUP BY p.id, p.name
ORDER BY total_books DESC, avg_rating DESC;

-- 10. Co-Authored Books Analysis
-- Purpose: Understand collaboration patterns and their success
SELECT
    b.id,
    b.name AS book_title,
    b.average_rating,
    b.total_reviews,
    COUNT(ba.author_id) AS author_count,
    STRING_AGG(a.name, ', ' ORDER BY a.name) AS authors
FROM books b
INNER JOIN book_authors ba ON b.id = ba.book_id
INNER JOIN authors a ON ba.author_id = a.id
WHERE b.is_deleted = FALSE
GROUP BY b.id, b.name, b.average_rating, b.total_reviews
HAVING COUNT(ba.author_id) > 1
ORDER BY b.average_rating DESC, b.total_reviews DESC;

-- ============================================
-- PRICING ANALYTICS
-- ============================================

-- 11. Price Range Distribution
-- Purpose: Understand pricing strategy and market positioning
SELECT
    CASE
        WHEN b.price_original_price < 10 THEN 'Budget (< $10)'
        WHEN b.price_original_price BETWEEN 10 AND 20 THEN 'Economy ($10-20)'
        WHEN b.price_original_price BETWEEN 20 AND 35 THEN 'Standard ($20-35)'
        WHEN b.price_original_price BETWEEN 35 AND 50 THEN 'Premium ($35-50)'
        ELSE 'Luxury (> $50)'
    END AS price_range,
    COUNT(*) AS book_count,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating,
    SUM(b.total_reviews) AS total_reviews
FROM books b
WHERE b.is_deleted = FALSE
    AND b.price_original_price IS NOT NULL
GROUP BY price_range
ORDER BY MIN(b.price_original_price);

-- 12. Discount Strategy Effectiveness
-- Purpose: Analyze if discounts correlate with higher engagement
SELECT
    CASE
        WHEN b.price_discount_price IS NULL THEN 'No Discount'
        WHEN ((b.price_original_price - b.price_discount_price) / b.price_original_price * 100) < 10 THEN 'Small (< 10%)'
        WHEN ((b.price_original_price - b.price_discount_price) / b.price_original_price * 100) BETWEEN 10 AND 25 THEN 'Medium (10-25%)'
        WHEN ((b.price_original_price - b.price_discount_price) / b.price_original_price * 100) BETWEEN 25 AND 50 THEN 'Large (25-50%)'
        ELSE 'Very Large (> 50%)'
    END AS discount_tier,
    COUNT(*) AS book_count,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating,
    ROUND(AVG(b.total_reviews)::numeric, 2) AS avg_reviews_per_book
FROM books b
WHERE b.is_deleted = FALSE
GROUP BY discount_tier
ORDER BY
    CASE discount_tier
        WHEN 'No Discount' THEN 1
        WHEN 'Small (< 10%)' THEN 2
        WHEN 'Medium (10-25%)' THEN 3
        WHEN 'Large (25-50%)' THEN 4
        ELSE 5
    END;

-- ============================================
-- QUALITY & ENGAGEMENT METRICS
-- ============================================

-- 13. Books with Rating-Review Mismatch
-- Purpose: Identify potential fake reviews or quality issues
SELECT
    b.id,
    b.name,
    b.average_rating,
    b.total_reviews,
    c.name AS category,
    CASE
        WHEN b.average_rating >= 4.5 AND b.total_reviews < 5 THEN 'High rating, low reviews - Verify quality'
        WHEN b.average_rating < 3.0 AND b.total_reviews > 50 THEN 'Low rating, many reviews - Quality issue'
        WHEN b.average_rating >= 4.0 AND b.total_reviews > 100 THEN 'Proven bestseller'
    END AS quality_flag
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
WHERE b.is_deleted = FALSE
    AND (
        (b.average_rating >= 4.5 AND b.total_reviews < 5) OR
        (b.average_rating < 3.0 AND b.total_reviews > 50) OR
        (b.average_rating >= 4.0 AND b.total_reviews > 100)
    )
ORDER BY b.total_reviews DESC;

-- 14. New Releases Performance (Last 90 Days)
-- Purpose: Track how new additions are performing
SELECT
    b.id,
    b.name,
    b.created_at,
    b.average_rating,
    b.total_reviews,
    b.price_original_price,
    c.name AS category,
    p.name AS publisher,
    EXTRACT(DAY FROM (NOW() - b.created_at)) AS days_in_catalog
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = FALSE
    AND b.created_at >= NOW() - INTERVAL '90 days'
ORDER BY b.total_reviews DESC, b.average_rating DESC;

-- ============================================
-- OPERATIONAL QUERIES
-- ============================================

-- 15. Orphaned Books (No Category or Publisher)
-- Purpose: Data quality check for incomplete records
SELECT
    b.id,
    b.name,
    b.category_id,
    b.publisher_id,
    b.created_at,
    CASE
        WHEN b.category_id IS NULL AND b.publisher_id IS NULL THEN 'Missing Both'
        WHEN b.category_id IS NULL THEN 'Missing Category'
        WHEN b.publisher_id IS NULL THEN 'Missing Publisher'
    END AS data_issue
FROM books b
WHERE b.is_deleted = FALSE
    AND (b.category_id IS NULL OR b.publisher_id IS NULL)
ORDER BY b.created_at DESC;

-- 16. Books with No Authors
-- Purpose: Data integrity check
SELECT
    b.id,
    b.name,
    b.created_at,
    c.name AS category,
    p.name AS publisher
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
LEFT JOIN book_authors ba ON b.id = ba.book_id
WHERE b.is_deleted = FALSE
    AND ba.id IS NULL
ORDER BY b.created_at DESC;

-- 17. Soft Deleted Books Report
-- Purpose: Review deleted items for potential recovery or final purge
SELECT
    b.id,
    b.name,
    b.last_modified_at AS deleted_at,
    b.average_rating,
    b.total_reviews,
    c.name AS category,
    p.name AS publisher
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = TRUE
ORDER BY b.last_modified_at DESC;

-- ============================================
-- TRENDING & RECOMMENDATIONS
-- ============================================

-- 18. Rising Stars (Good ratings with growing review counts)
-- Purpose: Identify books gaining traction for marketing campaigns
SELECT
    b.id,
    b.name,
    b.average_rating,
    b.total_reviews,
    b.price_original_price,
    c.name AS category,
    p.name AS publisher,
    ROUND(((b.average_rating * b.total_reviews) / NULLIF(EXTRACT(DAY FROM (NOW() - b.created_at)), 0))::numeric, 2) AS momentum_score
FROM books b
LEFT JOIN categories c ON b.category_id = c.id
LEFT JOIN publishers p ON b.publisher_id = p.id
WHERE b.is_deleted = FALSE
    AND b.average_rating >= 4.0
    AND b.total_reviews >= 10
    AND b.created_at >= NOW() - INTERVAL '180 days'
ORDER BY momentum_score DESC
LIMIT 20;

-- 19. Books Frequently Bought Together (Same Authors)
-- Purpose: Create product recommendations and bundles
SELECT
    a.name AS author,
    COUNT(ba.book_id) AS books_by_author,
    STRING_AGG(DISTINCT b.name, ' | ' ORDER BY b.name) AS book_titles,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating,
    SUM(b.total_reviews) AS total_reviews
FROM authors a
INNER JOIN book_authors ba ON a.id = ba.author_id
INNER JOIN books b ON ba.book_id = b.id
WHERE b.is_deleted = FALSE
GROUP BY a.id, a.name
HAVING COUNT(ba.book_id) >= 2
ORDER BY books_by_author DESC, total_reviews DESC
LIMIT 15;

-- 20. Category Cross-Sell Opportunities
-- Purpose: Find authors who write in multiple categories for cross-promotion
SELECT
    a.name AS author,
    COUNT(DISTINCT b.category_id) AS categories_count,
    STRING_AGG(DISTINCT c.name, ', ' ORDER BY c.name) AS categories,
    COUNT(DISTINCT ba.book_id) AS total_books,
    ROUND(AVG(b.average_rating)::numeric, 2) AS avg_rating
FROM authors a
INNER JOIN book_authors ba ON a.id = ba.author_id
INNER JOIN books b ON ba.book_id = b.id AND b.is_deleted = FALSE
LEFT JOIN categories c ON b.category_id = c.id
GROUP BY a.id, a.name
HAVING COUNT(DISTINCT b.category_id) > 1
ORDER BY categories_count DESC, total_books DESC;
