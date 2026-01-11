// Type definitions for BookWorm K6 performance tests

export type SortOption = {
	orderBy: string;
	isDescending: boolean;
};

export type SearchParams = {
	search?: string;
	minPrice?: number;
	maxPrice?: number;
	pageIndex?: number;
	pageSize?: number;
	orderBy?: string;
	isDescending?: boolean;
	category?: string;
};

export type BookItem = {
	id: string;
	title: string;
	price: number;
	[key: string]: unknown;
};

export type PagedResponse<T> = {
	items: T[];
	pageIndex: number;
	pageSize: number;
	totalItems: number;
	totalPages?: number;
};

export type TestData = {
	searchTerms: string[];
	categoryFilters: string[];
	sortOptions: SortOption[];
	authorNames: string[];
	publisherNames: string[];
};

export type TestScenario = {
	name: string;
	execute: () => void;
};
export type TestConstants = {
	HTTP_OK: number;
	HTTP_BAD_REQUEST: number;
	HTTP_NOT_FOUND: number;
	RESPONSE_TIME_THRESHOLD_95: number;
	RESPONSE_TIME_THRESHOLD_99: number;
	CHECK_RATE_THRESHOLD: number;
};

export type TestEndpoint = {
	url: string;
	name: string;
	maxDuration?: number;
};

export type TestConfiguration = {
	baseUrl: string;
	randomSeed: number;
};

// K6 Summary Types
export type K6MetricValues = {
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
};

export type K6Metric = {
	type: string;
	contains: string;
	values: K6MetricValues;
	thresholds?: Record<string, boolean>;
};

export type K6SummaryData = {
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
};
