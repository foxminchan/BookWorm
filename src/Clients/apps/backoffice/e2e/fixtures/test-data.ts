import { faker } from "@faker-js/faker";

/**
 * Test data generators for backoffice e2e tests
 */

export const testData = {
  /**
   * Generate test admin credentials
   */
  admin: {
    email: process.env.TEST_ADMIN_EMAIL || "admin@bookworm.test",
    password: process.env.TEST_ADMIN_PASSWORD || "Test@1234",
  },

  /**
   * Generate test book data
   */
  book: {
    title: () => faker.lorem.words(3),
    author: () => faker.person.fullName(),
    isbn: () => faker.string.numeric(13),
    price: () => Number.parseFloat(faker.commerce.price({ min: 10, max: 100 })),
    description: () => faker.lorem.paragraph(),
    category: () =>
      faker.helpers.arrayElement([
        "Fiction",
        "Non-Fiction",
        "Science",
        "History",
        "Biography",
      ]),
    publisher: () => faker.company.name(),
    publicationYear: () => faker.date.past({ years: 10 }).getFullYear(),
    stock: () => faker.number.int({ min: 0, max: 100 }),
  },

  /**
   * Generate test customer data
   */
  customer: {
    name: () => faker.person.fullName(),
    email: () => faker.internet.email(),
    phone: () => faker.phone.number(),
    address: () => faker.location.streetAddress(),
    city: () => faker.location.city(),
    state: () => faker.location.state(),
    zipCode: () => faker.location.zipCode(),
  },

  /**
   * Generate test order data
   */
  order: {
    orderId: () => faker.string.uuid(),
    orderNumber: () => faker.string.numeric(8),
    status: () =>
      faker.helpers.arrayElement([
        "Pending",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled",
      ]),
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
 * Commonly used test credentials
 */
export const credentials = {
  validAdmin: {
    email: "admin@bookworm.test",
    password: "Test@1234",
  },
  invalidAdmin: {
    email: "invalid@bookworm.test",
    password: "WrongPassword",
  },
};

/**
 * Test URLs
 */
export const urls = {
  home: "/",
  login: "/api/auth/signin",
  dashboard: "/",
  books: "/catalog/books",
  customers: "/ordering/customers",
  orders: "/ordering/orders",
  reviews: "/rating/reviews",
};
