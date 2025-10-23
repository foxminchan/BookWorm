// Type definitions for BookWorm K6 performance tests

export interface SortOption {
	orderBy: string;
	isDescending: boolean;
}

export interface SearchParams {
	search?: string;
	minPrice?: number;
	maxPrice?: number;
	pageIndex?: number;
	pageSize?: number;
	orderBy?: string;
	isDescending?: boolean;
	category?: string;
}

export interface BookItem {
	id: string;
	title: string;
	price: number;
	[key: string]: unknown;
}

export interface PagedResponse<T> {
	items: T[];
	pageIndex: number;
	pageSize: number;
	totalItems: number;
	totalPages?: number;
}

export interface TestData {
	searchTerms: string[];
	categoryFilters: string[];
	sortOptions: SortOption[];
	authorNames: string[];
	publisherNames: string[];
}

export interface TestScenario {
	name: string;
	execute: () => void;
}
export interface TestConstants {
	HTTP_OK: number;
	HTTP_BAD_REQUEST: number;
	HTTP_NOT_FOUND: number;
	RESPONSE_TIME_THRESHOLD_95: number;
	RESPONSE_TIME_THRESHOLD_99: number;
	CHECK_RATE_THRESHOLD: number;
}

export interface TestEndpoint {
	url: string;
	name: string;
	maxDuration?: number;
}

export interface TestConfiguration {
	baseUrl: string;
	randomSeed: number;
}

// K6 Summary Types
export interface K6MetricValues {
	avg?: number;
	min?: number;
	med?: number;
	max?: number;
	"p(90)"?: number;
	"p(95)"?: number;
	"p(99)"?: number;
	count?: number;
	rate?: number;
	passes?: number;
	fails?: number;
}

export interface K6Metric {
	type: string;
	contains: string;
	values: K6MetricValues;
	thresholds?: Record<string, boolean>;
}

export interface K6SummaryData {
	metrics: {
		http_reqs?: K6Metric;
		http_req_failed?: K6Metric;
		http_req_duration?: K6Metric;
		checks?: K6Metric;
		[key: string]: K6Metric | undefined;
	};
	state: {
		testRunDurationMs: number;
		[key: string]: unknown;
	};
	[key: string]: unknown;
}
