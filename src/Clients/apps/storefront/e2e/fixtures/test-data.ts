/**
 * Test data fixtures for E2E testing
 * Uses actual application types from @workspace/types
 * Integrates Faker for realistic data generation
 */
import { faker } from "@faker-js/faker";

import type { BasketItem, CustomerBasket } from "@workspace/types/basket/index";
import type { Author } from "@workspace/types/catalog/authors";
import type { Book } from "@workspace/types/catalog/books";
import type { Category } from "@workspace/types/catalog/categories";
import type { Publisher } from "@workspace/types/catalog/publishers";
import type { Buyer } from "@workspace/types/ordering/buyers";
import type {
  Order,
  OrderDetail,
  OrderStatus,
} from "@workspace/types/ordering/orders";

/**
 * Seed faker for test data generation
 * - Uses FAKER_SEED environment variable if provided for deterministic data
 * - Falls back to default seed (12345) for consistent test runs
 * - Set FAKER_SEED to different values or omit for varied test scenarios
 */
const fakerSeed = process.env.FAKER_SEED
  ? parseInt(process.env.FAKER_SEED, 10)
  : 12345;
faker.seed(fakerSeed);

/**
 * Test Authors
 */
export const testAuthors: Author[] = [
  { id: faker.string.uuid(), name: "Don Norman" },
  { id: faker.string.uuid(), name: "Robert C. Martin" },
  { id: faker.string.uuid(), name: "James Clear" },
  { id: faker.string.uuid(), name: "Test Author" },
  { id: faker.string.uuid(), name: "Martin Fowler" },
];

/**
 * Test Categories
 */
export const testCategories: Category[] = [
  { id: faker.string.uuid(), name: "Fiction" },
  { id: faker.string.uuid(), name: "Art & Design" },
  { id: faker.string.uuid(), name: "Technology" },
  { id: faker.string.uuid(), name: "Self-Help" },
  { id: faker.string.uuid(), name: "Science" },
  { id: faker.string.uuid(), name: "History" },
];

/**
 * Test Publishers
 */
export const testPublishers: Publisher[] = [
  { id: faker.string.uuid(), name: "Basic Books" },
  { id: faker.string.uuid(), name: "Prentice Hall" },
  { id: faker.string.uuid(), name: "Avery" },
  { id: faker.string.uuid(), name: "Random House" },
  { id: faker.string.uuid(), name: "O'Reilly Media" },
];

/**
 * Test Books - Using actual Book type from application
 */
export const testBooks: Book[] = [
  {
    id: faker.string.uuid(),
    name: "The Design of Everyday Things",
    description:
      "A powerful primer on how design serves as the communication between object and user.",
    imageUrl: "/images/books/design-everyday-things.jpg",
    price: 29.99,
    priceSale: 24.99,
    status: "InStock",
    category: testCategories[1]!, // Art & Design
    publisher: testPublishers[0]!, // Basic Books
    authors: [testAuthors[0]!], // Don Norman
    averageRating: 4.5,
    totalReviews: 1250,
  },
  {
    id: faker.string.uuid(),
    name: "Clean Code",
    description: "A Handbook of Agile Software Craftsmanship",
    imageUrl: "/images/books/clean-code.jpg",
    price: 49.99,
    priceSale: null,
    status: "InStock",
    category: testCategories[2]!, // Technology
    publisher: testPublishers[1]!, // Prentice Hall
    authors: [testAuthors[1]!], // Robert C. Martin
    averageRating: 4.8,
    totalReviews: 3200,
  },
  {
    id: faker.string.uuid(),
    name: "Atomic Habits",
    description: "An Easy & Proven Way to Build Good Habits & Break Bad Ones",
    imageUrl: "/images/books/atomic-habits.jpg",
    price: 27.99,
    priceSale: null,
    status: "InStock",
    category: testCategories[3]!, // Self-Help
    publisher: testPublishers[2]!, // Avery
    authors: [testAuthors[2]!], // James Clear
    averageRating: 4.9,
    totalReviews: 8500,
  },
  {
    id: faker.string.uuid(),
    name: "Out of Stock Book",
    description: "This book is currently unavailable",
    imageUrl: "/images/books/out-of-stock.jpg",
    price: 19.99,
    priceSale: null,
    status: "OutOfStock",
    category: testCategories[0]!, // Fiction
    publisher: testPublishers[3]!, // Random House
    authors: [testAuthors[3]!], // Test Author
    averageRating: 0,
    totalReviews: 0,
  },
  {
    id: faker.string.uuid(),
    name: "Refactoring",
    description: "Improving the Design of Existing Code",
    imageUrl: "/images/books/refactoring.jpg",
    price: 54.99,
    priceSale: 44.99,
    status: "InStock",
    category: testCategories[2]!, // Technology
    publisher: testPublishers[4]!, // O'Reilly
    authors: [testAuthors[4]!], // Martin Fowler
    averageRating: 4.7,
    totalReviews: 2100,
  },
];

/**
 * Test Buyer - Using actual Buyer type with Faker-generated data
 */
export const testBuyer: Buyer = {
  id: faker.string.uuid(),
  name: faker.person.fullName(),
  address: `${faker.location.streetAddress()}, ${faker.location.city()}, ${faker.location.state()}`,
};

/**
 * Test User Context (for authentication/session)
 */
export const testUser = {
  id: faker.string.uuid(),
  email: faker.internet.email(),
  firstName: faker.person.firstName(),
  lastName: faker.person.lastName(),
  buyerId: testBuyer.id,
};

/**
 * Test Basket Items
 */
export const testBasketItems: BasketItem[] = [
  {
    id: testBooks[0]!.id,
    quantity: 2,
    name: testBooks[0]!.name ?? undefined,
    price: testBooks[0]!.price,
    priceSale: testBooks[0]!.priceSale,
  },
  {
    id: testBooks[1]!.id,
    quantity: 1,
    name: testBooks[1]!.name ?? undefined,
    price: testBooks[1]!.price,
    priceSale: testBooks[1]!.priceSale,
  },
];

/**
 * Test Customer Basket
 */
export const testBasket: CustomerBasket = {
  id: faker.string.uuid(),
  items: testBasketItems,
};

/**
 * Test Orders
 */
export const testOrders: Order[] = [
  {
    id: faker.string.uuid(),
    date: new Date(Date.now() - 86400000 * 2).toISOString(), // 2 days ago
    total: 104.97,
    status: "Completed",
  },
  {
    id: faker.string.uuid(),
    date: new Date(Date.now() - 86400000 * 5).toISOString(), // 5 days ago
    total: 49.99,
    status: "New",
  },
  {
    id: faker.string.uuid(),
    date: new Date(Date.now() - 86400000 * 10).toISOString(), // 10 days ago
    total: 79.97,
    status: "Cancelled",
  },
];

/**
 * Test Order Detail
 */
export const testOrderDetail: OrderDetail = {
  id: testOrders[0]!.id,
  date: testOrders[0]!.date,
  total: testOrders[0]!.total,
  status: testOrders[0]!.status,
  items: [
    {
      id: testBooks[0]!.id,
      quantity: 2,
      price: testBooks[0]!.priceSale || testBooks[0]!.price,
      name: testBooks[0]!.name,
    },
    {
      id: testBooks[1]!.id,
      quantity: 1,
      price: testBooks[1]!.price,
      name: testBooks[1]!.name,
    },
  ],
};

/**
 * Helper: Generate multiple books for pagination testing with Faker
 */
export function generateBooks(count: number): Book[] {
  const books: Book[] = [];
  for (let i = 0; i < count; i++) {
    const categoryIndex = i % testCategories.length;
    const publisherIndex = i % testPublishers.length;
    const authorIndex = i % testAuthors.length;
    const price = faker.number.float({ min: 15, max: 75, fractionDigits: 2 });
    const hasSale = faker.datatype.boolean({ probability: 0.3 });

    books.push({
      id: faker.string.uuid(),
      name: faker.lorem.words({ min: 2, max: 5 }),
      description: faker.lorem.paragraph({ min: 1, max: 3 }),
      imageUrl: faker.image.url(),
      price,
      priceSale: hasSale
        ? faker.number.float({
            min: price * 0.6,
            max: price * 0.85,
            fractionDigits: 2,
          })
        : null,
      status: faker.helpers.arrayElement(["InStock", "OutOfStock"] as const),
      category: testCategories[categoryIndex]!,
      publisher: testPublishers[publisherIndex]!,
      authors: [testAuthors[authorIndex]!],
      averageRating: faker.number.float({ min: 3, max: 5, fractionDigits: 1 }),
      totalReviews: faker.number.int({ min: 10, max: 5000 }),
    });
  }
  return books;
}

/**
 * Helper: Generate basket items from books
 */
export function createBasketItem(book: Book, quantity: number = 1): BasketItem {
  return {
    id: book.id,
    quantity,
    name: book.name ?? undefined,
    price: book.price,
    priceSale: book.priceSale,
  };
}

/**
 * Helper: Calculate basket total
 */
export function calculateBasketTotal(items: BasketItem[]): number {
  return items.reduce((total, item) => {
    const price = item.priceSale ?? item.price;
    return total + price * item.quantity;
  }, 0);
}

/**
 * Helper: Create test order from basket with Faker-generated ID
 */
export function createTestOrder(
  basket: CustomerBasket,
  status: OrderStatus = "New",
): OrderDetail {
  const total = calculateBasketTotal(basket.items);
  const shippingCost = faker.number.float({
    min: 3.99,
    max: 9.99,
    fractionDigits: 2,
  });

  return {
    id: faker.string.uuid(),
    date: faker.date.recent({ days: 30 }).toISOString(),
    total: total + shippingCost,
    status,
    items: basket.items.map((item) => ({
      id: item.id,
      quantity: item.quantity,
      price: item.priceSale ?? item.price,
      name: item.name ?? null,
    })),
  };
}

/**
 * Factory: Create a random Author
 */
export function createRandomAuthor(): Author {
  return {
    id: faker.string.uuid(),
    name: faker.person.fullName(),
  };
}

/**
 * Factory: Create a random Publisher
 */
export function createRandomPublisher(): Publisher {
  return {
    id: faker.string.uuid(),
    name: faker.company.name(),
  };
}

/**
 * Factory: Create a random Book with realistic data
 */
export function createRandomBook(overrides?: Partial<Book>): Book {
  const price = faker.number.float({ min: 15, max: 75, fractionDigits: 2 });
  const hasSale = faker.datatype.boolean({ probability: 0.3 });

  return {
    id: faker.string.uuid(),
    name: faker.commerce.productName(),
    description: faker.commerce.productDescription(),
    imageUrl: faker.image.url(),
    price,
    priceSale: hasSale
      ? faker.number.float({
          min: price * 0.6,
          max: price * 0.85,
          fractionDigits: 2,
        })
      : null,
    status: faker.helpers.arrayElement(["InStock", "OutOfStock"] as const),
    category: faker.helpers.arrayElement(testCategories),
    publisher: faker.helpers.arrayElement(testPublishers),
    authors: [faker.helpers.arrayElement(testAuthors)!],
    averageRating: faker.number.float({ min: 3, max: 5, fractionDigits: 1 }),
    totalReviews: faker.number.int({ min: 10, max: 5000 }),
    ...overrides,
  };
}

/**
 * Factory: Create a random Buyer
 */
export function createRandomBuyer(): Buyer {
  return {
    id: faker.string.uuid(),
    name: faker.person.fullName(),
    address: `${faker.location.streetAddress()}, ${faker.location.city()}, ${faker.location.state()}, ${faker.location.zipCode()}, ${faker.location.country()}`,
  };
}
