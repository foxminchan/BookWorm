import { v7 as uuidv7 } from "uuid";
import type { Author } from "@workspace/types/catalog/authors";
import authorsData from "../../data/authors.json";

export const mockAuthors: Author[] = authorsData;

export const authorsStore = {
  authors: [...mockAuthors],

  list(): Author[] {
    return [...this.authors];
  },

  get(id: string): Author | undefined {
    return this.authors.find((author) => author.id === id);
  },

  create(name: string, id?: string): string {
    const newId = id || uuidv7();
    this.authors.push({
      id: newId,
      name,
    });
    return newId;
  },

  update(id: string, name: string): boolean {
    const index = this.authors.findIndex((author) => author.id === id);
    if (index === -1) return false;

    this.authors[index] = {
      id,
      name,
    };
    return true;
  },

  delete(id: string): boolean {
    const index = this.authors.findIndex((author) => author.id === id);
    if (index === -1) return false;

    this.authors.splice(index, 1);
    return true;
  },

  reset(): void {
    this.authors = [...mockAuthors];
  },
};
