import { v7 as uuidv7 } from "uuid";
import type { Publisher } from "@workspace/types/catalog/publishers";
import publishersData from "@/data/publishers.json";

export const mockPublishers: Publisher[] = publishersData;

export const publishersStore = {
  publishers: [...mockPublishers],

  list(): Publisher[] {
    return [...this.publishers];
  },

  create(name: string, id?: string): string {
    const newId = id || uuidv7();
    this.publishers.push({
      id: newId,
      name,
    });
    return newId;
  },

  update(id: string, name: string): boolean {
    const index = this.publishers.findIndex((publisher) => publisher.id === id);
    if (index === -1) return false;

    this.publishers[index] = {
      id,
      name,
    };
    return true;
  },

  delete(id: string): boolean {
    const index = this.publishers.findIndex((publisher) => publisher.id === id);
    if (index === -1) return false;

    this.publishers.splice(index, 1);
    return true;
  },

  reset(): void {
    this.publishers = [...mockPublishers];
  },
};
