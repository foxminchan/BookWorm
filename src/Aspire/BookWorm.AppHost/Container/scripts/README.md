# BookWorm K6 Performance Testing Suite

This directory contains comprehensive performance testing scripts for the BookWorm application using K6.

## Features

- **Multiple Test Scenarios**: Five different testing scenarios covering various load patterns
- **Chai Assertions**: Enhanced validation using Chai assertion library alongside traditional K6 checks
- **Realistic Data**: Random test data generators for search terms, categories, authors, and publishers
- **Error Handling**: Retry mechanisms and comprehensive error validation
- **Detailed Reporting**: HTML and JSON output with custom summary statistics

## Test Scenarios

### 1. Browse Catalog (`browse_catalog`)

- **Purpose**: Simulates users browsing the bookstore catalog
- **Load**: Light load (10-15 VUs)
- **Duration**: 3 minutes
- **Behavior**:
  - Lists books and categories
  - Views specific categories (70% chance)
  - Browses authors (50% chance)
  - Views individual book details (30% chance)
  - Tests pagination (40% chance)

### 2. Search & Filter (`search_filter`)

- **Purpose**: Tests search functionality and filtering capabilities
- **Load**: Medium load (20-30 VUs)
- **Duration**: 3 minutes
- **Behavior**:
  - Comprehensive search with multiple parameters
  - Price range filtering
  - Category filtering
  - Sorting options testing
  - Pagination with different page sizes

### 3. API Comprehensive (`api_comprehensive`)

- **Purpose**: Systematic testing of all API endpoints
- **Load**: Medium load (15-25 VUs)
- **Duration**: 2 minutes
- **Behavior**:
  - Tests all catalog endpoints
  - Various parameter combinations
  - Edge case testing (boundary conditions)
  - Error scenario validation
  - Malformed parameter handling

### 4. Stress Test (`stress_test`)

- **Purpose**: High-intensity testing with concurrent requests
- **Load**: High load (50-100 VUs)
- **Duration**: 5 minutes
- **Behavior**:
  - Multiple concurrent requests
  - Complex search scenarios
  - More lenient response time thresholds

### 5. Spike Test (`spike_test`)

- **Purpose**: Tests system resilience during sudden traffic spikes
- **Load**: Sudden spike (10 → 80 → 10 VUs)
- **Duration**: 2 minutes
- **Behavior**:
  - Rapid traffic increase simulation
  - Consecutive request testing
  - Recovery behavior validation

## Performance Thresholds

The test includes comprehensive performance thresholds:

### Response Time Thresholds

- **95th percentile**: < 500ms (general), with endpoint-specific variations
- **99th percentile**: < 1000ms
- **Average**: < 200ms
- **Median**: < 150ms

### Error Rate Thresholds

- **HTTP errors**: < 1%
- **Check failures**: < 5%

### Endpoint-Specific Thresholds

- **Books**: 95th percentile < 600ms, average < 250ms
- **Search**: 95th percentile < 800ms, average < 300ms
- **Categories**: 95th percentile < 300ms, average < 150ms
- **Authors/Publishers**: 95th percentile < 400ms, average < 200ms

### Scenario-Specific Thresholds

- **Stress Test**: 95th percentile < 1000ms
- **Spike Test**: 95th percentile < 1500ms

## Output Files

After running tests, you'll get:

- **summary.html**: Detailed HTML report with charts and graphs
- **summary.json**: Machine-readable JSON output for further analysis
- **Console output**: Real-time metrics and summary statistics

The tests generate comprehensive reports showing performance metrics, response times, and validation results. Below is an example of the HTML summary report:

![Summary HTML Output](../../../../../assets/k6-report.png)

## Test Data

The script uses realistic test data including:

- **Search Terms**: book, programming, fiction, science, history, mystery, romance, fantasy, thriller, biography
- **Categories**: Technology, Fiction, Science, History, Business, Romance, Mystery, Fantasy, Biography
- **Sort Options**: Name (asc/desc), Price (asc/desc), Rating (desc), PublishDate (desc)
- **Price Ranges**: Random prices between $5-$100
- **Page Sizes**: 5, 10, 15, 20, 25 items per page

## Monitoring and Analysis

### Key Metrics to Monitor

1. **Response Times**: Ensure all endpoints meet performance thresholds
2. **Error Rates**: Monitor for any failures or timeouts
3. **Throughput**: Requests per second under different loads
4. **Resource Utilization**: CPU, memory, and network usage on the server
5. **User Experience**: Validate that the application remains responsive under load
