import { faker } from "@faker-js/faker";

export const BASE_PATH = "/backoffice";

/**
 * Test data generators for backoffice e2e tests
 */
export const testData = {
  /**
   * Generate test admin user data for mocked session
   */
  admin: {
    id: "admin-test-id",
    name: "Admin User",
    email: process.env.TEST_ADMIN_EMAIL || "admin@bookworm.test",
  },

  /**
   * Generate test book data
   */
  book: {
    name: () => faker.lorem.words(3),
    price: () => Number.parseFloat(faker.commerce.price({ min: 10, max: 100 })),
    priceSale: () =>
      Number.parseFloat(faker.commerce.price({ min: 5, max: 50 })),
    description: () => faker.lorem.paragraph(),
  },

  /**
   * Generate test customer data
   */
  customer: {
    name: () => faker.person.fullName(),
    address: () => faker.location.streetAddress(),
  },

  /**
   * Generate test order data
   */
  order: {
    orderId: () => faker.string.uuid(),
    status: () => faker.helpers.arrayElement(["New", "Completed", "Cancelled"]),
    total: () => Number.parseFloat(faker.commerce.price({ min: 20, max: 500 })),
    date: () => faker.date.recent({ days: 30 }),
  },

  /**
   * Generate test review data
   */
  review: {
    rating: () => faker.number.int({ min: 1, max: 5 }),
    comment: () => faker.lorem.paragraph(),
    reviewerName: () => faker.person.fullName(),
    date: () => faker.date.recent({ days: 60 }),
  },
};

/**
 * Test URLs (prefixed with Next.js basePath "/backoffice")
 */
export const urls = {
  home: BASE_PATH,
  dashboard: BASE_PATH,
  books: `${BASE_PATH}/books`,
  newBook: `${BASE_PATH}/books/new`,
  authors: `${BASE_PATH}/authors`,
  categories: `${BASE_PATH}/categories`,
  publishers: `${BASE_PATH}/publishers`,
  customers: `${BASE_PATH}/customers`,
  orders: `${BASE_PATH}/orders`,
  reviews: `${BASE_PATH}/reviews`,
};
