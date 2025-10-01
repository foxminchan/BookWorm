-- Business Intelligence Queries for Rating Database
-- These queries provide valuable insights for understanding customer feedback and satisfaction

-- ============================================
-- OVERALL SATISFACTION METRICS
-- ============================================

-- 1. Overall Rating Distribution
-- Purpose: Understand the general satisfaction level across all feedback
SELECT
    rating,
    COUNT(*) as feedback_count,
    ROUND(COUNT(*)::numeric / SUM(COUNT(*)) OVER () * 100, 2) as percentage,
    ROUND(AVG(LENGTH(comment))::numeric, 0) as avg_comment_length
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY rating
ORDER BY rating DESC;

-- 2. Net Promoter Score (NPS) Calculation
-- Purpose: Calculate NPS based on rating distribution (9-10 = Promoters, 7-8 = Passives, 0-6 = Detractors)
-- Note: Assuming 5-star system maps to 0-10 scale (5=10, 4=8, 3=6, 2=4, 1=2)
SELECT
    COUNT(CASE WHEN rating >= 5 THEN 1 END) as promoters,
    COUNT(CASE WHEN rating = 4 OR rating = 3 THEN 1 END) as passives,
    COUNT(CASE WHEN rating <= 2 THEN 1 END) as detractors,
    COUNT(*) as total_responses,
    ROUND(
        (COUNT(CASE WHEN rating >= 5 THEN 1 END)::numeric - COUNT(CASE WHEN rating <= 2 THEN 1 END)::numeric) /
        COUNT(*)::numeric * 100,
        2
    ) as nps_score
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days';

-- 3. Average Rating Over Time
-- Purpose: Track satisfaction trends for continuous improvement
SELECT
    DATE(created_at) as feedback_date,
    COUNT(*) as total_feedbacks,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    COUNT(CASE WHEN rating >= 4 THEN 1 END) as positive_feedbacks,
    COUNT(CASE WHEN rating <= 2 THEN 1 END) as negative_feedbacks,
    ROUND(
        COUNT(CASE WHEN rating >= 4 THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as satisfaction_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '90 days'
GROUP BY DATE(created_at)
ORDER BY feedback_date DESC;

-- 4. Monthly Rating Trends
-- Purpose: Long-term satisfaction tracking and seasonal patterns
SELECT
    DATE_TRUNC('month', created_at) as month,
    COUNT(*) as feedback_count,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    MIN(rating) as min_rating,
    MAX(rating) as max_rating,
    MODE() WITHIN GROUP (ORDER BY rating) as most_common_rating,
    ROUND(STDDEV(rating)::numeric, 2) as rating_std_dev
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '12 months'
GROUP BY DATE_TRUNC('month', created_at)
ORDER BY month DESC;

-- ============================================
-- FEEDBACK VOLUME ANALYSIS
-- ============================================

-- 5. Feedback Volume by Time of Day
-- Purpose: Identify when customers are most engaged in providing feedback
SELECT
    EXTRACT(HOUR FROM created_at) as hour_of_day,
    COUNT(*) as feedback_count,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 0 THEN 1 END) as feedbacks_with_comments
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY EXTRACT(HOUR FROM created_at)
ORDER BY hour_of_day;

-- 6. Feedback Volume by Day of Week
-- Purpose: Understand weekly patterns for resource allocation
SELECT
    TO_CHAR(created_at, 'Day') as day_of_week,
    EXTRACT(DOW FROM created_at) as day_number,
    COUNT(*) as feedback_count,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    COUNT(CASE WHEN rating <= 2 THEN 1 END) as negative_feedbacks
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '60 days'
GROUP BY TO_CHAR(created_at, 'Day'), EXTRACT(DOW FROM created_at)
ORDER BY day_number;

-- 7. Feedback Response Rate
-- Purpose: Track customer engagement over time
SELECT
    DATE_TRUNC('week', created_at) as week,
    COUNT(*) as total_feedbacks,
    COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 10 THEN 1 END) as detailed_feedbacks,
    ROUND(
        COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 10 THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as detailed_feedback_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '12 weeks'
GROUP BY DATE_TRUNC('week', created_at)
ORDER BY week DESC;

-- ============================================
-- CUSTOMER FEEDBACK ANALYSIS
-- ============================================

-- 8. Most Active Reviewers
-- Purpose: Identify power users and brand advocates
SELECT
    first_name,
    last_name,
    COUNT(*) as total_feedbacks,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    MIN(created_at) as first_feedback_date,
    MAX(created_at) as last_feedback_date,
    COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 50 THEN 1 END) as detailed_reviews,
    EXTRACT(DAY FROM (MAX(created_at) - MIN(created_at))) as days_active
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '180 days'
GROUP BY first_name, last_name
HAVING COUNT(*) > 1
ORDER BY total_feedbacks DESC, avg_rating DESC
LIMIT 50;

-- 9. First-Time vs Repeat Reviewers
-- Purpose: Understand reviewer behavior patterns
SELECT
    CASE
        WHEN review_count = 1 THEN 'First-time reviewer'
        WHEN review_count BETWEEN 2 AND 3 THEN 'Occasional reviewer'
        WHEN review_count BETWEEN 4 AND 10 THEN 'Regular reviewer'
        ELSE 'Power reviewer'
    END as reviewer_type,
    COUNT(DISTINCT reviewer) as unique_reviewers,
    SUM(review_count) as total_reviews,
    ROUND(AVG(avg_rating)::numeric, 2) as avg_rating
FROM (
    SELECT
        CONCAT(first_name, ' ', last_name) as reviewer,
        COUNT(*) as review_count,
        AVG(rating) as avg_rating
    FROM feedbacks
    WHERE created_at >= NOW() - INTERVAL '90 days'
    GROUP BY CONCAT(first_name, ' ', last_name)
) reviewer_stats
GROUP BY reviewer_type
ORDER BY
    CASE reviewer_type
        WHEN 'First-time reviewer' THEN 1
        WHEN 'Occasional reviewer' THEN 2
        WHEN 'Regular reviewer' THEN 3
        ELSE 4
    END;

-- 10. Customer Sentiment Evolution
-- Purpose: Track how individual customer sentiment changes over time
SELECT
    first_name,
    last_name,
    COUNT(*) as total_reviews,
    ROUND(AVG(rating)::numeric, 2) as overall_avg_rating,
    ROUND(AVG(CASE WHEN created_at >= NOW() - INTERVAL '30 days' THEN rating END)::numeric, 2) as recent_avg_rating,
    ROUND(
        AVG(CASE WHEN created_at >= NOW() - INTERVAL '30 days' THEN rating END)::numeric -
        AVG(rating)::numeric,
        2
    ) as sentiment_change
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '180 days'
GROUP BY first_name, last_name
HAVING COUNT(*) >= 3
ORDER BY sentiment_change DESC NULLS LAST
LIMIT 30;

-- ============================================
-- COMMENT ANALYSIS
-- ============================================

-- 11. Comment Engagement Rate
-- Purpose: Understand how many customers provide detailed feedback
SELECT
    rating,
    COUNT(*) as total_feedbacks,
    COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 0 THEN 1 END) as with_comments,
    COUNT(CASE WHEN comment IS NULL OR LENGTH(comment) = 0 THEN 1 END) as without_comments,
    ROUND(
        COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 0 THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as comment_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY rating
ORDER BY rating DESC;

-- 12. Comment Length Analysis by Rating
-- Purpose: Understand correlation between satisfaction and feedback detail
SELECT
    rating,
    COUNT(CASE WHEN comment IS NOT NULL AND LENGTH(comment) > 0 THEN 1 END) as feedbacks_with_comments,
    ROUND(AVG(CASE WHEN comment IS NOT NULL THEN LENGTH(comment) END)::numeric, 0) as avg_comment_length,
    MIN(CASE WHEN comment IS NOT NULL THEN LENGTH(comment) END) as min_comment_length,
    MAX(CASE WHEN comment IS NOT NULL THEN LENGTH(comment) END) as max_comment_length,
    CASE
        WHEN rating >= 4 THEN 'Positive'
        WHEN rating = 3 THEN 'Neutral'
        ELSE 'Negative'
    END as sentiment
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
    AND comment IS NOT NULL
    AND LENGTH(comment) > 0
GROUP BY rating
ORDER BY rating DESC;

-- 13. Detailed Feedback (Long Comments)
-- Purpose: Identify customers providing comprehensive feedback
SELECT
    id,
    first_name,
    last_name,
    rating,
    LENGTH(comment) as comment_length,
    LEFT(comment, 100) as comment_preview,
    created_at
FROM feedbacks
WHERE comment IS NOT NULL
    AND LENGTH(comment) > 200
    AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY LENGTH(comment) DESC, created_at DESC
LIMIT 50;

-- 14. Silent Ratings (No Comments)
-- Purpose: Identify patterns in ratings without explanatory text
SELECT
    rating,
    COUNT(*) as silent_ratings,
    ROUND(COUNT(*)::numeric / SUM(COUNT(*)) OVER () * 100, 2) as percentage,
    DATE(created_at) as date
FROM feedbacks
WHERE (comment IS NULL OR LENGTH(comment) = 0)
    AND created_at >= NOW() - INTERVAL '30 days'
GROUP BY rating, DATE(created_at)
ORDER BY date DESC, rating DESC;

-- ============================================
-- NEGATIVE FEEDBACK FOCUS
-- ============================================

-- 15. Critical Feedback Requiring Attention
-- Purpose: Identify urgent negative feedback for immediate action
SELECT
    id,
    first_name,
    last_name,
    rating,
    comment,
    created_at,
    EXTRACT(HOUR FROM (NOW() - created_at)) as hours_since_feedback
FROM feedbacks
WHERE rating <= 2
    AND created_at >= NOW() - INTERVAL '7 days'
ORDER BY created_at DESC;

-- 16. Negative Feedback Trends
-- Purpose: Track negative feedback patterns over time
SELECT
    DATE(created_at) as date,
    COUNT(CASE WHEN rating = 1 THEN 1 END) as one_star,
    COUNT(CASE WHEN rating = 2 THEN 1 END) as two_star,
    COUNT(CASE WHEN rating <= 2 THEN 1 END) as total_negative,
    COUNT(*) as total_feedbacks,
    ROUND(
        COUNT(CASE WHEN rating <= 2 THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as negative_feedback_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '60 days'
GROUP BY DATE(created_at)
ORDER BY date DESC;

-- 17. Common Issues in Negative Feedback
-- Purpose: Extract patterns from negative feedback comments (requires manual review)
SELECT
    rating,
    comment,
    first_name,
    last_name,
    created_at
FROM feedbacks
WHERE rating <= 2
    AND comment IS NOT NULL
    AND LENGTH(comment) > 20
    AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC
LIMIT 100;

-- ============================================
-- POSITIVE FEEDBACK INSIGHTS
-- ============================================

-- 18. Top Rated Experiences
-- Purpose: Identify what delights customers for replication
SELECT
    id,
    first_name,
    last_name,
    rating,
    comment,
    created_at
FROM feedbacks
WHERE rating >= 5
    AND comment IS NOT NULL
    AND LENGTH(comment) > 30
    AND created_at >= NOW() - INTERVAL '30 days'
ORDER BY created_at DESC, LENGTH(comment) DESC
LIMIT 50;

-- 19. Testimonial Candidates
-- Purpose: Find positive, detailed feedback suitable for marketing
SELECT
    id,
    CONCAT(first_name, ' ', SUBSTRING(last_name, 1, 1), '.') as reviewer_name,
    rating,
    comment,
    LENGTH(comment) as comment_length,
    created_at
FROM feedbacks
WHERE rating >= 5
    AND comment IS NOT NULL
    AND LENGTH(comment) BETWEEN 50 AND 500
    AND created_at >= NOW() - INTERVAL '90 days'
ORDER BY rating DESC, LENGTH(comment) DESC
LIMIT 30;

-- 20. Positive Feedback Growth Rate
-- Purpose: Track improvement in customer satisfaction
SELECT
    DATE_TRUNC('week', created_at) as week,
    COUNT(CASE WHEN rating >= 4 THEN 1 END) as positive_feedbacks,
    COUNT(*) as total_feedbacks,
    ROUND(
        COUNT(CASE WHEN rating >= 4 THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as positive_rate,
    LAG(
        ROUND(
            COUNT(CASE WHEN rating >= 4 THEN 1 END)::numeric /
            COUNT(*)::numeric * 100,
            2
        )
    ) OVER (ORDER BY DATE_TRUNC('week', created_at)) as previous_week_positive_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '12 weeks'
GROUP BY DATE_TRUNC('week', created_at)
ORDER BY week DESC;

-- ============================================
-- STATISTICAL ANALYSIS
-- ============================================

-- 21. Rating Distribution Statistics
-- Purpose: Comprehensive statistical overview of ratings
SELECT
    COUNT(*) as total_feedbacks,
    ROUND(AVG(rating)::numeric, 2) as mean_rating,
    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY rating) as median_rating,
    MODE() WITHIN GROUP (ORDER BY rating) as mode_rating,
    MIN(rating) as min_rating,
    MAX(rating) as max_rating,
    ROUND(STDDEV(rating)::numeric, 2) as std_deviation,
    ROUND(VARIANCE(rating)::numeric, 2) as variance,
    PERCENTILE_CONT(0.25) WITHIN GROUP (ORDER BY rating) as q1_rating,
    PERCENTILE_CONT(0.75) WITHIN GROUP (ORDER BY rating) as q3_rating
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days';

-- 22. Rating Volatility Analysis
-- Purpose: Measure consistency of ratings over time
SELECT
    DATE_TRUNC('week', created_at) as week,
    COUNT(*) as feedback_count,
    ROUND(AVG(rating)::numeric, 2) as avg_rating,
    ROUND(STDDEV(rating)::numeric, 2) as rating_volatility,
    ROUND(
        (STDDEV(rating) / NULLIF(AVG(rating), 0))::numeric,
        2
    ) as coefficient_of_variation
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '26 weeks'
GROUP BY DATE_TRUNC('week', created_at)
ORDER BY week DESC;

-- 23. Outlier Detection
-- Purpose: Identify unusual rating patterns
WITH rating_stats AS (
    SELECT
        AVG(rating) as mean_rating,
        STDDEV(rating) as std_rating
    FROM feedbacks
    WHERE created_at >= NOW() - INTERVAL '30 days'
)
SELECT
    f.id,
    f.first_name,
    f.last_name,
    f.rating,
    f.comment,
    f.created_at,
    ROUND(ABS(f.rating - rs.mean_rating) / NULLIF(rs.std_rating, 0), 2) as z_score
FROM feedbacks f
CROSS JOIN rating_stats rs
WHERE f.created_at >= NOW() - INTERVAL '30 days'
    AND ABS(f.rating - rs.mean_rating) > (2 * rs.std_rating)
ORDER BY z_score DESC;

-- ============================================
-- OPERATIONAL METRICS
-- ============================================

-- 24. Feedback Processing Backlog
-- Purpose: Track feedback that may need follow-up
SELECT
    rating,
    COUNT(*) as feedback_count,
    COUNT(CASE WHEN last_modified_at IS NULL THEN 1 END) as unprocessed,
    COUNT(CASE WHEN last_modified_at IS NOT NULL THEN 1 END) as processed,
    ROUND(
        COUNT(CASE WHEN last_modified_at IS NOT NULL THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as processing_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '7 days'
GROUP BY rating
ORDER BY rating;

-- 25. Response Time Analysis
-- Purpose: Measure time between feedback creation and processing
SELECT
    rating,
    COUNT(CASE WHEN last_modified_at IS NOT NULL THEN 1 END) as processed_count,
    ROUND(
        AVG(
            EXTRACT(EPOCH FROM (last_modified_at - created_at)) / 3600
        )::numeric,
        2
    ) as avg_response_hours,
    ROUND(
        PERCENTILE_CONT(0.5) WITHIN GROUP (
            ORDER BY EXTRACT(EPOCH FROM (last_modified_at - created_at)) / 3600
        )::numeric,
        2
    ) as median_response_hours,
    MAX(
        EXTRACT(EPOCH FROM (last_modified_at - created_at)) / 3600
    )::integer as max_response_hours
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
    AND last_modified_at IS NOT NULL
    AND last_modified_at > created_at
GROUP BY rating
ORDER BY rating DESC;

-- ============================================
-- COHORT & RETENTION ANALYSIS
-- ============================================

-- 26. Reviewer Cohort Analysis
-- Purpose: Track reviewer retention and engagement over time
SELECT
    DATE_TRUNC('month', first_review) as cohort_month,
    COUNT(DISTINCT reviewer) as cohort_size,
    COUNT(DISTINCT CASE WHEN months_since_first = 1 THEN reviewer END) as month_1_active,
    COUNT(DISTINCT CASE WHEN months_since_first = 2 THEN reviewer END) as month_2_active,
    COUNT(DISTINCT CASE WHEN months_since_first = 3 THEN reviewer END) as month_3_active,
    ROUND(
        COUNT(DISTINCT CASE WHEN months_since_first = 1 THEN reviewer END)::numeric /
        COUNT(DISTINCT reviewer)::numeric * 100,
        2
    ) as month_1_retention_rate
FROM (
    SELECT
        CONCAT(first_name, ' ', last_name) as reviewer,
        MIN(created_at) as first_review,
        created_at,
        FLOOR(
            EXTRACT(EPOCH FROM (created_at - MIN(created_at) OVER (PARTITION BY CONCAT(first_name, ' ', last_name)))) /
            2592000
        ) as months_since_first
    FROM feedbacks
    WHERE created_at >= NOW() - INTERVAL '6 months'
    GROUP BY CONCAT(first_name, ' ', last_name), created_at
) cohort_data
GROUP BY DATE_TRUNC('month', first_review)
ORDER BY cohort_month DESC;

-- 27. Customer Lifetime Value (CLV) by Rating Pattern
-- Purpose: Understand value of different customer segments
SELECT
    CASE
        WHEN avg_rating >= 4.5 THEN 'Promoters (4.5-5.0)'
        WHEN avg_rating >= 3.5 THEN 'Satisfied (3.5-4.4)'
        WHEN avg_rating >= 2.5 THEN 'Neutral (2.5-3.4)'
        ELSE 'Detractors (<2.5)'
    END as customer_segment,
    COUNT(DISTINCT reviewer) as customer_count,
    ROUND(AVG(review_count)::numeric, 2) as avg_reviews_per_customer,
    ROUND(AVG(avg_rating)::numeric, 2) as segment_avg_rating,
    ROUND(SUM(review_count)::numeric / COUNT(DISTINCT reviewer)::numeric, 2) as engagement_score
FROM (
    SELECT
        CONCAT(first_name, ' ', last_name) as reviewer,
        COUNT(*) as review_count,
        AVG(rating) as avg_rating
    FROM feedbacks
    WHERE created_at >= NOW() - INTERVAL '180 days'
    GROUP BY CONCAT(first_name, ' ', last_name)
) customer_metrics
GROUP BY customer_segment
ORDER BY
    CASE customer_segment
        WHEN 'Promoters (4.5-5.0)' THEN 1
        WHEN 'Satisfied (3.5-4.4)' THEN 2
        WHEN 'Neutral (2.5-3.4)' THEN 3
        ELSE 4
    END;

-- ============================================
-- DATA QUALITY CHECKS
-- ============================================

-- 28. Data Completeness Check
-- Purpose: Monitor data quality and completeness
SELECT
    COUNT(*) as total_records,
    COUNT(CASE WHEN first_name IS NULL OR first_name = '' THEN 1 END) as missing_first_name,
    COUNT(CASE WHEN last_name IS NULL OR last_name = '' THEN 1 END) as missing_last_name,
    COUNT(CASE WHEN comment IS NULL OR comment = '' THEN 1 END) as missing_comment,
    COUNT(CASE WHEN created_at IS NULL THEN 1 END) as missing_created_date,
    ROUND(
        COUNT(CASE WHEN comment IS NOT NULL AND comment != '' THEN 1 END)::numeric /
        COUNT(*)::numeric * 100,
        2
    ) as comment_completion_rate
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days';

-- 29. Duplicate Detection
-- Purpose: Identify potential duplicate feedback submissions
SELECT
    first_name,
    last_name,
    rating,
    LEFT(comment, 50) as comment_preview,
    COUNT(*) as occurrence_count,
    STRING_AGG(id::text, ', ') as feedback_ids,
    MIN(created_at) as first_submission,
    MAX(created_at) as last_submission
FROM feedbacks
WHERE created_at >= NOW() - INTERVAL '30 days'
GROUP BY first_name, last_name, rating, LEFT(comment, 50)
HAVING COUNT(*) > 1
ORDER BY occurrence_count DESC, last_submission DESC;

-- 30. Feedback Volume Anomaly Detection
-- Purpose: Identify unusual spikes or drops in feedback volume
WITH daily_counts AS (
    SELECT
        DATE(created_at) as date,
        COUNT(*) as feedback_count
    FROM feedbacks
    WHERE created_at >= NOW() - INTERVAL '60 days'
    GROUP BY DATE(created_at)
),
stats AS (
    SELECT
        AVG(feedback_count) as avg_count,
        STDDEV(feedback_count) as std_count
    FROM daily_counts
)
SELECT
    dc.date,
    dc.feedback_count,
    ROUND(s.avg_count::numeric, 2) as avg_daily_count,
    ROUND((dc.feedback_count - s.avg_count) / NULLIF(s.std_count, 0), 2) as z_score,
    CASE
        WHEN ABS((dc.feedback_count - s.avg_count) / NULLIF(s.std_count, 0)) > 2 THEN 'Anomaly'
        ELSE 'Normal'
    END as status
FROM daily_counts dc
CROSS JOIN stats s
ORDER BY dc.date DESC
LIMIT 30;
