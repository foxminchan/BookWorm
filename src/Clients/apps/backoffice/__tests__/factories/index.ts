import { faker } from "@faker-js/faker";

/**
 * Factory functions for generating realistic test data using faker
 */

export function createMockAuthor(
  overrides: Partial<{ id: string; name: string }> = {},
) {
  return {
    id: overrides.id ?? faker.string.uuid(),
    name: overrides.name ?? faker.person.fullName(),
  };
}

export function createMockCategory(overrides = {}) {
  return {
    id: faker.string.uuid(),
    name: faker.word.words(1),
    ...overrides,
  };
}

export function createMockPublisher(overrides = {}) {
  return {
    id: faker.string.uuid(),
    name: faker.company.name(),
    ...overrides,
  };
}

export function createMockBook(overrides = {}) {
  const price = faker.number.float({ min: 10, max: 50, fractionDigits: 2 });
  const priceSale = faker.datatype.boolean()
    ? faker.number.float({ min: 5, max: price - 1, fractionDigits: 2 })
    : null;

  return {
    id: faker.string.uuid(),
    name: faker.book.title(),
    description: faker.lorem.paragraph(),
    imageUrl: faker.image.url(),
    price,
    priceSale,
    status: faker.helpers.arrayElement(["InStock", "OutOfStock"]),
    category: createMockCategory(),
    publisher: createMockPublisher(),
    authors: [createMockAuthor(), createMockAuthor()],
    averageRating: faker.number.float({ min: 0, max: 5, fractionDigits: 1 }),
    totalReviews: faker.number.int({ min: 0, max: 500 }),
    ...overrides,
  };
}

export function createMockCustomer(overrides = {}) {
  return {
    id: faker.string.uuid(),
    email: faker.internet.email(),
    name: faker.person.fullName(),
    phone: faker.phone.number(),
    address: faker.location.streetAddress(),
    ...overrides,
  };
}

export function createMockOrder(overrides = {}) {
  return {
    id: faker.string.uuid(),
    date: faker.date.past().toISOString(),
    total: faker.number.float({ min: 20, max: 500, fractionDigits: 2 }),
    status: faker.helpers.arrayElement(["New", "Completed", "Cancelled"]),
    ...overrides,
  };
}

export function createMockReview(overrides = {}) {
  return {
    id: faker.string.uuid(),
    bookId: faker.string.uuid(),
    rating: faker.number.int({ min: 1, max: 5 }),
    title: faker.lorem.sentence(),
    comment: faker.lorem.paragraph(),
    author: faker.person.fullName(),
    createdAt: faker.date.past(),
    ...overrides,
  };
}

export function createMockOrderItem(overrides = {}) {
  return {
    id: faker.string.uuid(),
    quantity: faker.number.int({ min: 1, max: 10 }),
    price: faker.number.float({ min: 5, max: 50, fractionDigits: 2 }),
    name: faker.book.title(),
    ...overrides,
  };
}

export function createMockFeedback(overrides = {}) {
  return {
    id: faker.string.uuid(),
    firstName: faker.person.firstName(),
    lastName: faker.person.lastName(),
    rating: faker.number.float({ min: 1, max: 5, fractionDigits: 1 }),
    comment: faker.lorem.paragraph(),
    bookId: faker.string.uuid(),
    ...overrides,
  };
}
