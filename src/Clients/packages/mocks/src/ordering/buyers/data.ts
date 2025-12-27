import { v7 as uuidv7 } from "uuid";
import type { Buyer } from "@workspace/types/ordering/buyers";

export const MOCK_USER_ID = uuidv7();

const mockBuyers: Buyer[] = [
  {
    id: MOCK_USER_ID,
    name: "John Doe",
    address: "123 Main St, New York, NY",
  },
  {
    id: uuidv7(),
    name: "Jane Smith",
    address: "456 Oak Ave, Los Angeles, CA",
  },
  {
    id: uuidv7(),
    name: "Bob Johnson",
    address: "789 Pine Rd, Chicago, IL",
  },
  {
    id: uuidv7(),
    name: "Alice Williams",
    address: "321 Elm St, Houston, TX",
  },
  {
    id: uuidv7(),
    name: "Charlie Brown",
    address: "654 Maple Dr, Phoenix, AZ",
  },
];

let buyersStore = [...mockBuyers];

export const buyersStoreManager = {
  getAll: (): Buyer[] => {
    return buyersStore;
  },

  getById: (id: string): Buyer | undefined => {
    return buyersStore.find((b) => b.id === id);
  },

  getCurrentBuyer: (): Buyer | undefined => {
    return buyersStore.find((b) => b.id === MOCK_USER_ID);
  },

  create: (
    name: string,
    street: string,
    city: string,
    province: string,
  ): Buyer => {
    const newBuyer: Buyer = {
      id: uuidv7(),
      name,
      address: `${street}, ${city}, ${province}`,
    };
    buyersStore.push(newBuyer);
    return newBuyer;
  },

  updateAddress: (
    id: string,
    street: string,
    city: string,
    province: string,
  ): Buyer | undefined => {
    const buyer = buyersStore.find((b) => b.id === id);
    if (!buyer) return undefined;

    buyer.address = `${street}, ${city}, ${province}`;
    return buyer;
  },

  delete: (id: string): boolean => {
    const index = buyersStore.findIndex((b) => b.id === id);
    if (index === -1) return false;

    buyersStore.splice(index, 1);
    return true;
  },

  reset: () => {
    buyersStore = [...mockBuyers];
  },
};
