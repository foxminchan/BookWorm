import { v7 as uuidv7 } from "uuid";
import type { PublisherDto } from "@workspace/types/catalog/publishers";

export const mockPublishers: PublisherDto[] = [
  {
    id: uuidv7(),
    name: "Penguin Random House",
  },
  {
    id: uuidv7(),
    name: "HarperCollins",
  },
  {
    id: uuidv7(),
    name: "Simon & Schuster",
  },
  {
    id: uuidv7(),
    name: "Macmillan Publishers",
  },
  {
    id: uuidv7(),
    name: "Hachette Book Group",
  },
  {
    id: uuidv7(),
    name: "Scholastic",
  },
  {
    id: uuidv7(),
    name: "Pearson",
  },
  {
    id: uuidv7(),
    name: "Oxford University Press",
  },
  {
    id: uuidv7(),
    name: "Cambridge University Press",
  },
  {
    id: uuidv7(),
    name: "Bloomsbury Publishing",
  },
];

export const publishersStore = {
  publishers: [...mockPublishers],

  getAll(): PublisherDto[] {
    return [...this.publishers];
  },

  getById(id: string): PublisherDto | undefined {
    return this.publishers.find((publisher) => publisher.id === id);
  },

  create(name: string): string {
    const newId = uuidv7();
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
