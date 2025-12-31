import { v7 as uuidv7 } from "uuid";
import type { Category } from "@workspace/types/catalog/categories";
import categoriesData from "../../data/categories.json";

export const mockCategories: Category[] = categoriesData;

export const categoriesStore = {
  categories: [...mockCategories],

  list(): Category[] {
    return [...this.categories];
  },

  create(name: string, id?: string): string {
    const newId = id || uuidv7();
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
