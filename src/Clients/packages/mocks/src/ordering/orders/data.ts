import { v7 as uuidv7 } from "uuid";
import type {
  Order,
  OrderDetail,
  OrderItem,
  OrderStatus,
} from "@workspace/types/ordering/orders";
import { mockBooks } from "../../catalog/books/data";

export const MOCK_USER_ID = uuidv7();

const createOrderItem = (bookIndex: number, quantity: number): OrderItem => {
  const book = mockBooks[bookIndex];
  if (!book) {
    throw new Error(`Book at index ${bookIndex} not found`);
  }
  return {
    id: book.id,
    quantity,
    price: book.priceSale ?? book.price,
    name: book.name,
  };
};

const mockOrders: OrderDetail[] = [
  {
    id: uuidv7(),
    date: new Date(Date.now() - 86400000 * 2).toISOString(),
    total: 59.97,
    status: "Completed",
    items: [
      createOrderItem(0, 2), // Harry Potter
      createOrderItem(2, 1), // 1984
    ],
  },
  {
    id: uuidv7(),
    date: new Date(Date.now() - 86400000).toISOString(),
    total: 51.48,
    status: "New",
    items: [
      createOrderItem(1, 1), // The Hobbit
      createOrderItem(4, 2), // And Then There Were None
    ],
  },
  {
    id: uuidv7(),
    date: new Date().toISOString(),
    total: 62.48,
    status: "New",
    items: [
      createOrderItem(0, 1), // Harry Potter
      createOrderItem(1, 1), // The Hobbit
      createOrderItem(2, 1), // 1984
    ],
  },
  {
    id: uuidv7(),
    date: new Date(Date.now() - 86400000 * 5).toISOString(),
    total: 38.98,
    status: "Cancelled",
    items: [
      createOrderItem(4, 2), // And Then There Were None
      createOrderItem(2, 1), // 1984
    ],
  },
];

let ordersStore = [...mockOrders];

export const ordersStoreManager = {
  getAll: (): OrderDetail[] => {
    return ordersStore;
  },

  getById: (id: string): OrderDetail | undefined => {
    return ordersStore.find((o) => o.id === id);
  },

  create: (items: OrderItem[]): OrderDetail => {
    const total = items.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0,
    );
    const newOrder: OrderDetail = {
      id: uuidv7(),
      date: new Date().toISOString(),
      total,
      status: "New",
      items,
    };
    ordersStore.push(newOrder);
    return newOrder;
  },

  updateStatus: (id: string, status: OrderStatus): OrderDetail | undefined => {
    const order = ordersStore.find((o) => o.id === id);
    if (!order) return undefined;

    order.status = status;
    return order;
  },

  delete: (id: string): boolean => {
    const index = ordersStore.findIndex((o) => o.id === id);
    if (index === -1) return false;

    ordersStore.splice(index, 1);
    return true;
  },

  reset: () => {
    ordersStore = [...mockOrders];
  },
};
