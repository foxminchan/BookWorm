import { v7 as uuidv7 } from "uuid";
import type { Buyer } from "@workspace/types/ordering/buyers";
import buyersData from "../../data/buyers.json";

const mockBuyers: Buyer[] = buyersData;

export const MOCK_USER_ID = mockBuyers[0]?.id || "";

let buyersStore = [...mockBuyers];

export const buyersStoreManager = {
  list: (): Buyer[] => {
    return buyersStore;
  },

  get: (id: string): Buyer | undefined => {
    return buyersStore.find((b) => b.id === id);
  },

  getCurrent: (): Buyer | undefined => {
    return buyersStore.find((b) => b.id === MOCK_USER_ID);
  },

  create: (
    name: string,
    street: string,
    city: string,
    province: string,
    id?: string,
  ): Buyer => {
    const newBuyer: Buyer = {
      id: id || uuidv7(),
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
