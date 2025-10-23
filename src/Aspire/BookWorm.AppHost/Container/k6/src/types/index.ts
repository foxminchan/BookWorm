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
	[key: string]: any;
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
