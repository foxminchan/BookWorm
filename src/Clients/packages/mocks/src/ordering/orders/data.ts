import type {
  OrderDetail,
  OrderItem,
  OrderStatus,
} from "@workspace/types/ordering/orders";

import orderItemsData from "../../data/orderItems.json";
import ordersData from "../../data/orders.json";

const mockOrders: OrderDetail[] = ordersData.map((order) => {
  const orderItems = orderItemsData.find((oi) => oi.orderId === order.id);
  return {
    ...order,
    status: order.status as OrderStatus,
    items: orderItems?.items || [],
  };
});

let ordersStore = [...mockOrders];

export const ordersStoreManager = {
  list: (): OrderDetail[] => {
    return ordersStore;
  },

  get: (id: string): OrderDetail | undefined => {
    return ordersStore.find((o) => o.id === id);
  },

  create: (items: OrderItem[], id?: string): OrderDetail => {
    const total = items.reduce(
      (sum, item) => sum + item.price * item.quantity,
      0,
    );
    const newOrder: OrderDetail = {
      id: id || crypto.randomUUID(),
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
