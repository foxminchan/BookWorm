import type { TestData, SortOption } from "../types";
import type { SeededRandom } from "./seeded-random";

/**
 * Test data pools for realistic scenarios
 */
export const TEST_DATA: TestData = {
	searchTerms: [
		"book",
		"programming",
		"fiction",
		"science",
		"history",
		"mystery",
		"romance",
		"fantasy",
		"thriller",
		"biography",
	],
	categoryFilters: [
		"Technology",
		"Fiction",
		"Science",
		"History",
		"Business",
		"Romance",
		"Mystery",
		"Fantasy",
		"Biography",
	],
	sortOptions: [
		{ orderBy: "Name", isDescending: false },
		{ orderBy: "Name", isDescending: true },
		{ orderBy: "Price", isDescending: false },
		{ orderBy: "Price", isDescending: true },
		{ orderBy: "Rating", isDescending: true },
		{ orderBy: "PublishDate", isDescending: true },
	],
	authorNames: [
		"John Doe",
		"Jane Smith",
		"Robert Johnson",
		"Mary Wilson",
		"David Brown",
	],
	publisherNames: [
		"TechBooks",
		"Fiction House",
		"Academic Press",
		"Popular Books",
		"Classic Publishing",
	],
};

/**
 * Random data generators using seeded random for reproducible test runs
 */
export class TestDataGenerator {
	constructor(private readonly random: SeededRandom) {}

	getRandomSearchTerm(): string {
		return this.random.pick(TEST_DATA.searchTerms);
	}

	getRandomCategory(): string {
		return this.random.pick(TEST_DATA.categoryFilters);
	}

	getRandomSortOption(): SortOption {
		return this.random.pick(TEST_DATA.sortOptions);
	}

	getRandomPrice(min: number = 5, max: number = 100): number {
		return this.random.int(min, max);
	}

	getRandomPageSize(): number {
		return this.random.pick([5, 10, 15, 20, 25]);
	}

	getRandomPageIndex(): number {
		return this.random.int(1, 5);
	}
}
