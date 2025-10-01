-- Business Insight Queries for Ordering Database
-- These queries provide valuable business metrics and insights for decision-making

-- ============================================================================
-- ORDER PERFORMANCE METRICS
-- ============================================================================

-- 1. Daily Order Summary with Conversion Rates
-- Provides overview of order volume, revenue, and cancellation rates by day
SELECT
    DATE(created_at) as order_date,
    COUNT(*) as total_orders,
    COUNT(*) FILTER (WHERE status = 0) as new_orders,
    COUNT(*) FILTER (WHERE status = 2) as completed_orders,
    COUNT(*) FILTER (WHERE status = 1) as cancelled_orders,
    ROUND(COUNT(*) FILTER (WHERE status = 2)::NUMERIC / NULLIF(COUNT(*), 0) * 100, 2) as completion_rate_pct,
    ROUND(COUNT(*) FILTER (WHERE status = 1)::NUMERIC / NULLIF(COUNT(*), 0) * 100, 2) as cancellation_rate_pct,
    SUM(oi.price * oi.quantity) as total_revenue,
    AVG(oi.price * oi.quantity) as avg_order_value
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY DATE(created_at)
ORDER BY order_date DESC
LIMIT 90;

-- 2. Order Status Distribution
-- Shows current state of all orders in the system
SELECT
    CASE status
        WHEN 0 THEN 'New'
        WHEN 1 THEN 'Cancelled'
        WHEN 2 THEN 'Completed'
    END as order_status,
    COUNT(*) as order_count,
    ROUND(COUNT(*)::NUMERIC / SUM(COUNT(*)) OVER () * 100, 2) as percentage,
    SUM(oi.price * oi.quantity) as total_revenue
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY status
ORDER BY order_count DESC;

-- 3. Average Order Processing Time
-- Measures time from order creation to completion
SELECT
    DATE_TRUNC('week', o.created_at) as week,
    COUNT(*) as completed_orders,
    AVG(EXTRACT(EPOCH FROM (o.last_modified_at - o.created_at)) / 3600) as avg_hours_to_complete,
    MIN(EXTRACT(EPOCH FROM (o.last_modified_at - o.created_at)) / 3600) as min_hours_to_complete,
    MAX(EXTRACT(EPOCH FROM (o.last_modified_at - o.created_at)) / 3600) as max_hours_to_complete,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY EXTRACT(EPOCH FROM (o.last_modified_at - o.created_at)) / 3600) as median_hours_to_complete
FROM orders o
WHERE o.status = 2
  AND o.is_deleted = false
  AND o.last_modified_at IS NOT NULL
GROUP BY DATE_TRUNC('week', o.created_at)
ORDER BY week DESC
LIMIT 12;

-- ============================================================================
-- CUSTOMER BEHAVIOR ANALYSIS
-- ============================================================================

-- 4. Top Buyers by Order Volume and Revenue
-- Identifies most valuable customers
SELECT
    b.id as buyer_id,
    b.name as buyer_name,
    b.address_city as city,
    b.address_province as province,
    COUNT(o.id) as total_orders,
    COUNT(*) FILTER (WHERE o.status = 2) as completed_orders,
    COUNT(*) FILTER (WHERE o.status = 1) as cancelled_orders,
    SUM(oi.price * oi.quantity) as lifetime_revenue,
    AVG(oi.price * oi.quantity) as avg_order_value,
    MAX(o.created_at) as last_order_date,
    EXTRACT(DAY FROM NOW() - MAX(o.created_at)) as days_since_last_order
FROM buyers b
INNER JOIN orders o ON b.id = o.buyer_id
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY b.id, b.name, b.address_city, b.address_province
HAVING COUNT(o.id) > 0
ORDER BY lifetime_revenue DESC
LIMIT 100;

-- 5. Customer Segmentation by Order Frequency
-- Categorizes customers into segments for targeted marketing
SELECT
    CASE
        WHEN order_count >= 10 THEN 'VIP (10+ orders)'
        WHEN order_count >= 5 THEN 'Frequent (5-9 orders)'
        WHEN order_count >= 2 THEN 'Regular (2-4 orders)'
        ELSE 'One-time (1 order)'
    END as customer_segment,
    COUNT(*) as customer_count,
    AVG(order_count) as avg_orders_per_customer,
    SUM(total_revenue) as segment_revenue,
    AVG(total_revenue) as avg_customer_lifetime_value
FROM (
    SELECT
        b.id,
        COUNT(o.id) as order_count,
        SUM(oi.price * oi.quantity) as total_revenue
    FROM buyers b
    INNER JOIN orders o ON b.id = o.buyer_id
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.is_deleted = false AND o.status = 2
    GROUP BY b.id
) customer_metrics
GROUP BY customer_segment
ORDER BY
    CASE customer_segment
        WHEN 'VIP (10+ orders)' THEN 1
        WHEN 'Frequent (5-9 orders)' THEN 2
        WHEN 'Regular (2-4 orders)' THEN 3
        ELSE 4
    END;

-- 6. Customer Churn Analysis
-- Identifies customers who haven't ordered recently
SELECT
    b.id as buyer_id,
    b.name as buyer_name,
    COUNT(o.id) as total_orders,
    MAX(o.created_at) as last_order_date,
    EXTRACT(DAY FROM NOW() - MAX(o.created_at)) as days_since_last_order,
    CASE
        WHEN EXTRACT(DAY FROM NOW() - MAX(o.created_at)) > 180 THEN 'At Risk (6+ months inactive)'
        WHEN EXTRACT(DAY FROM NOW() - MAX(o.created_at)) > 90 THEN 'Dormant (3-6 months inactive)'
        WHEN EXTRACT(DAY FROM NOW() - MAX(o.created_at)) > 30 THEN 'Inactive (1-3 months)'
        ELSE 'Active'
    END as churn_risk,
    SUM(oi.price * oi.quantity) as lifetime_revenue
FROM buyers b
INNER JOIN orders o ON b.id = o.buyer_id
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY b.id, b.name
HAVING MAX(o.created_at) < NOW() - INTERVAL '30 days'
ORDER BY days_since_last_order DESC
LIMIT 100;

-- ============================================================================
-- PRODUCT PERFORMANCE ANALYSIS
-- ============================================================================

-- 7. Best Selling Books
-- Identifies top-performing products
SELECT
    oi.book_id,
    COUNT(DISTINCT o.id) as order_count,
    SUM(oi.quantity) as total_units_sold,
    SUM(oi.price * oi.quantity) as total_revenue,
    AVG(oi.price) as avg_price,
    COUNT(DISTINCT o.buyer_id) as unique_buyers
FROM order_items oi
INNER JOIN orders o ON oi.order_id = o.id
WHERE o.status = 2 AND o.is_deleted = false
GROUP BY oi.book_id
ORDER BY total_revenue DESC
LIMIT 50;

-- 8. Product Performance by Time Period
-- Trends for specific products over time
SELECT
    DATE_TRUNC('month', o.created_at) as month,
    oi.book_id,
    COUNT(DISTINCT o.id) as order_count,
    SUM(oi.quantity) as units_sold,
    SUM(oi.price * oi.quantity) as revenue,
    AVG(oi.price) as avg_price
FROM order_items oi
INNER JOIN orders o ON oi.order_id = o.id
WHERE o.status = 2
  AND o.is_deleted = false
  AND o.created_at >= NOW() - INTERVAL '12 months'
GROUP BY DATE_TRUNC('month', o.created_at), oi.book_id
ORDER BY month DESC, revenue DESC;

-- 9. Order Item Distribution
-- Analyzes order basket composition
SELECT
    item_count as items_per_order,
    COUNT(*) as order_count,
    ROUND(COUNT(*)::NUMERIC / SUM(COUNT(*)) OVER () * 100, 2) as percentage,
    AVG(total_value) as avg_order_value
FROM (
    SELECT
        o.id,
        COUNT(oi.id) as item_count,
        SUM(oi.price * oi.quantity) as total_value
    FROM orders o
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.status = 2 AND o.is_deleted = false
    GROUP BY o.id
) order_metrics
GROUP BY item_count
ORDER BY item_count;

-- ============================================================================
-- GEOGRAPHIC ANALYSIS
-- ============================================================================

-- 10. Revenue by Geographic Location
-- Shows performance across different regions
SELECT
    b.address_province as province,
    b.address_city as city,
    COUNT(DISTINCT b.id) as total_customers,
    COUNT(o.id) as total_orders,
    COUNT(*) FILTER (WHERE o.status = 2) as completed_orders,
    SUM(oi.price * oi.quantity) as total_revenue,
    AVG(oi.price * oi.quantity) as avg_order_value,
    ROUND(COUNT(*) FILTER (WHERE o.status = 2)::NUMERIC / NULLIF(COUNT(o.id), 0) * 100, 2) as completion_rate_pct
FROM buyers b
LEFT JOIN orders o ON b.id = o.buyer_id
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false OR o.is_deleted IS NULL
GROUP BY b.address_province, b.address_city
HAVING COUNT(o.id) > 0
ORDER BY total_revenue DESC
LIMIT 50;

-- 11. Top Cities by Order Volume
-- Identifies key markets for expansion
SELECT
    b.address_city as city,
    b.address_province as province,
    COUNT(DISTINCT b.id) as customer_base,
    COUNT(o.id) as total_orders,
    SUM(oi.price * oi.quantity) as total_revenue,
    AVG(oi.price * oi.quantity) as avg_order_value,
    COUNT(DISTINCT oi.book_id) as unique_products_ordered
FROM buyers b
INNER JOIN orders o ON b.id = o.buyer_id
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.status = 2
  AND o.is_deleted = false
  AND b.address_city IS NOT NULL
GROUP BY b.address_city, b.address_province
ORDER BY total_revenue DESC
LIMIT 20;

-- ============================================================================
-- OPERATIONAL EFFICIENCY METRICS
-- ============================================================================

-- 12. Order Cancellation Analysis
-- Investigates patterns in cancelled orders
SELECT
    DATE(o.created_at) as cancellation_date,
    COUNT(*) as cancelled_orders,
    AVG(EXTRACT(EPOCH FROM (o.last_modified_at - o.created_at)) / 60) as avg_minutes_to_cancel,
    COUNT(DISTINCT o.buyer_id) as affected_customers,
    SUM(oi.price * oi.quantity) as lost_revenue
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.status = 1
  AND o.is_deleted = false
  AND o.created_at >= NOW() - INTERVAL '90 days'
GROUP BY DATE(o.created_at)
ORDER BY cancellation_date DESC;

-- 13. Hourly Order Pattern
-- Shows peak ordering hours for capacity planning
SELECT
    EXTRACT(HOUR FROM created_at) as hour_of_day,
    EXTRACT(DOW FROM created_at) as day_of_week,
    COUNT(*) as order_count,
    AVG(total_value) as avg_order_value,
    COUNT(*) FILTER (WHERE status = 2) as completed_orders,
    COUNT(*) FILTER (WHERE status = 1) as cancelled_orders
FROM (
    SELECT
        o.id,
        o.created_at,
        o.status,
        SUM(oi.price * oi.quantity) as total_value
    FROM orders o
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.is_deleted = false
      AND o.created_at >= NOW() - INTERVAL '30 days'
    GROUP BY o.id, o.created_at, o.status
) order_data
GROUP BY EXTRACT(HOUR FROM created_at), EXTRACT(DOW FROM created_at)
ORDER BY day_of_week, hour_of_day;

-- 14. Monthly Growth Trends
-- Year-over-year and month-over-month comparison
SELECT
    DATE_TRUNC('month', o.created_at) as month,
    COUNT(*) as total_orders,
    COUNT(*) FILTER (WHERE o.status = 2) as completed_orders,
    SUM(oi.price * oi.quantity) as revenue,
    COUNT(DISTINCT o.buyer_id) as unique_customers,
    AVG(oi.price * oi.quantity) as avg_order_value,
    LAG(SUM(oi.price * oi.quantity)) OVER (ORDER BY DATE_TRUNC('month', o.created_at)) as prev_month_revenue,
    ROUND(
        (SUM(oi.price * oi.quantity) - LAG(SUM(oi.price * oi.quantity)) OVER (ORDER BY DATE_TRUNC('month', o.created_at)))
        / NULLIF(LAG(SUM(oi.price * oi.quantity)) OVER (ORDER BY DATE_TRUNC('month', o.created_at)), 0) * 100,
        2
    ) as revenue_growth_pct
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY DATE_TRUNC('month', o.created_at)
ORDER BY month DESC
LIMIT 24;

-- ============================================================================
-- REVENUE INSIGHTS
-- ============================================================================

-- 15. Revenue Breakdown by Order Status
-- Understanding revenue at risk from pending/cancelled orders
SELECT
    CASE status
        WHEN 0 THEN 'New (Pending)'
        WHEN 1 THEN 'Cancelled (Lost)'
        WHEN 2 THEN 'Completed (Realized)'
    END as order_status,
    COUNT(DISTINCT o.id) as order_count,
    SUM(oi.price * oi.quantity) as total_value,
    AVG(oi.price * oi.quantity) as avg_order_value,
    ROUND(SUM(oi.price * oi.quantity) / SUM(SUM(oi.price * oi.quantity)) OVER () * 100, 2) as revenue_percentage
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.is_deleted = false
GROUP BY o.status
ORDER BY o.status;

-- 16. Top Revenue Days
-- Identifies highest-earning days for understanding seasonality
SELECT
    DATE(o.created_at) as order_date,
    EXTRACT(DOW FROM o.created_at) as day_of_week,
    COUNT(DISTINCT o.id) as order_count,
    SUM(oi.price * oi.quantity) as daily_revenue,
    AVG(oi.price * oi.quantity) as avg_order_value,
    COUNT(DISTINCT o.buyer_id) as unique_customers
FROM orders o
LEFT JOIN order_items oi ON o.id = oi.order_id
WHERE o.status = 2
  AND o.is_deleted = false
  AND o.created_at >= NOW() - INTERVAL '6 months'
GROUP BY DATE(o.created_at), EXTRACT(DOW FROM o.created_at)
ORDER BY daily_revenue DESC
LIMIT 30;

-- ============================================================================
-- CUSTOMER LIFETIME VALUE (CLV)
-- ============================================================================

-- 17. Customer Lifetime Value Analysis
-- Comprehensive customer value metrics
WITH customer_metrics AS (
    SELECT
        b.id as buyer_id,
        b.name,
        MIN(o.created_at) as first_order_date,
        MAX(o.created_at) as last_order_date,
        COUNT(o.id) as total_orders,
        COUNT(*) FILTER (WHERE o.status = 2) as completed_orders,
        SUM(oi.price * oi.quantity) as lifetime_revenue,
        AVG(oi.price * oi.quantity) as avg_order_value,
        EXTRACT(DAY FROM MAX(o.created_at) - MIN(o.created_at)) as customer_lifespan_days
    FROM buyers b
    INNER JOIN orders o ON b.id = o.buyer_id
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.is_deleted = false
    GROUP BY b.id, b.name
)
SELECT
    buyer_id,
    name,
    first_order_date,
    last_order_date,
    total_orders,
    completed_orders,
    lifetime_revenue,
    avg_order_value,
    customer_lifespan_days,
    CASE
        WHEN customer_lifespan_days > 0
        THEN ROUND(lifetime_revenue / (customer_lifespan_days / 30.0), 2)
        ELSE lifetime_revenue
    END as avg_monthly_revenue,
    ROUND(lifetime_revenue / NULLIF(completed_orders, 0), 2) as revenue_per_order
FROM customer_metrics
WHERE completed_orders > 0
ORDER BY lifetime_revenue DESC
LIMIT 100;

-- 18. Cohort Analysis by First Order Month
-- Retention and revenue patterns by customer acquisition cohort
SELECT
    DATE_TRUNC('month', first_order_date) as cohort_month,
    COUNT(DISTINCT buyer_id) as cohort_size,
    SUM(lifetime_revenue) as total_cohort_revenue,
    AVG(lifetime_revenue) as avg_customer_value,
    AVG(total_orders) as avg_orders_per_customer,
    COUNT(*) FILTER (WHERE total_orders > 1) as repeat_customers,
    ROUND(COUNT(*) FILTER (WHERE total_orders > 1)::NUMERIC / COUNT(*) * 100, 2) as repeat_rate_pct
FROM (
    SELECT
        b.id as buyer_id,
        MIN(o.created_at) as first_order_date,
        COUNT(o.id) as total_orders,
        SUM(oi.price * oi.quantity) as lifetime_revenue
    FROM buyers b
    INNER JOIN orders o ON b.id = o.buyer_id
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.is_deleted = false AND o.status = 2
    GROUP BY b.id
) cohort_data
GROUP BY DATE_TRUNC('month', first_order_date)
ORDER BY cohort_month DESC
LIMIT 12;

-- ============================================================================
-- ORDER QUALITY METRICS
-- ============================================================================

-- 19. Average Items Per Order Trend
-- Tracks basket size over time
SELECT
    DATE_TRUNC('month', o.created_at) as month,
    AVG(item_count) as avg_items_per_order,
    AVG(total_value) as avg_order_value,
    AVG(total_value / item_count) as avg_value_per_item,
    COUNT(*) as order_count
FROM (
    SELECT
        o.id,
        o.created_at,
        COUNT(oi.id) as item_count,
        SUM(oi.price * oi.quantity) as total_value
    FROM orders o
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.status = 2 AND o.is_deleted = false
    GROUP BY o.id, o.created_at
) order_details
GROUP BY DATE_TRUNC('month', created_at)
ORDER BY month DESC
LIMIT 12;

-- 20. Order Value Distribution
-- Understanding order size segments
SELECT
    CASE
        WHEN total_value < 20 THEN 'Under $20'
        WHEN total_value < 50 THEN '$20-$50'
        WHEN total_value < 100 THEN '$50-$100'
        WHEN total_value < 200 THEN '$100-$200'
        ELSE 'Over $200'
    END as order_value_range,
    COUNT(*) as order_count,
    ROUND(COUNT(*)::NUMERIC / SUM(COUNT(*)) OVER () * 100, 2) as percentage,
    SUM(total_value) as total_revenue,
    AVG(total_value) as avg_order_value
FROM (
    SELECT
        o.id,
        SUM(oi.price * oi.quantity) as total_value
    FROM orders o
    LEFT JOIN order_items oi ON o.id = oi.order_id
    WHERE o.status = 2 AND o.is_deleted = false
    GROUP BY o.id
) order_values
GROUP BY order_value_range
ORDER BY
    CASE order_value_range
        WHEN 'Under $20' THEN 1
        WHEN '$20-$50' THEN 2
        WHEN '$50-$100' THEN 3
        WHEN '$100-$200' THEN 4
        ELSE 5
    END;
