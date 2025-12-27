import { v7 as uuidv7 } from "uuid";
import type { Category } from "@workspace/types/catalog/categories";

export const mockCategories: Category[] = [
  {
    id: uuidv7(),
    name: "Fiction",
  },
  {
    id: uuidv7(),
    name: "Non-Fiction",
  },
  {
    id: uuidv7(),
    name: "Science Fiction",
  },
  {
    id: uuidv7(),
    name: "Fantasy",
  },
  {
    id: uuidv7(),
    name: "Mystery",
  },
  {
    id: uuidv7(),
    name: "Thriller",
  },
  {
    id: uuidv7(),
    name: "Romance",
  },
  {
    id: uuidv7(),
    name: "Biography",
  },
  {
    id: uuidv7(),
    name: "History",
  },
  {
    id: uuidv7(),
    name: "Self-Help",
  },
];

export const categoriesStore = {
  categories: [...mockCategories],

  getAll(): Category[] {
    return [...this.categories];
  },

  getById(id: string): Category | undefined {
    return this.categories.find((category) => category.id === id);
  },

  create(name: string): string {
    const newId = uuidv7();
    this.categories.push({
      id: newId,
      name,
    });
    return newId;
  },

  update(id: string, name: string): boolean {
    const index = this.categories.findIndex((category) => category.id === id);
    if (index === -1) return false;

    this.categories[index] = {
      id,
      name,
    };
    return true;
  },

  delete(id: string): boolean {
    const index = this.categories.findIndex((category) => category.id === id);
    if (index === -1) return false;

    this.categories.splice(index, 1);
    return true;
  },

  reset(): void {
    this.categories = [...mockCategories];
  },
};
