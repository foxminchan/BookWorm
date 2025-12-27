import { v7 as uuidv7 } from "uuid";
import type { Author } from "@workspace/types/catalog/authors";

export const mockAuthors: Author[] = [
  {
    id: uuidv7(),
    name: "J.K. Rowling",
  },
  {
    id: uuidv7(),
    name: "George R.R. Martin",
  },
  {
    id: uuidv7(),
    name: "J.R.R. Tolkien",
  },
  {
    id: uuidv7(),
    name: "Stephen King",
  },
  {
    id: uuidv7(),
    name: "Agatha Christie",
  },
  {
    id: uuidv7(),
    name: "Isaac Asimov",
  },
  {
    id: uuidv7(),
    name: "Terry Pratchett",
  },
  {
    id: uuidv7(),
    name: "Neil Gaiman",
  },
  {
    id: uuidv7(),
    name: "Brandon Sanderson",
  },
  {
    id: uuidv7(),
    name: "Patrick Rothfuss",
  },
];

export const authorsStore = {
  authors: [...mockAuthors],

  getAll(): Author[] {
    return [...this.authors];
  },

  getById(id: string): Author | undefined {
    return this.authors.find((author) => author.id === id);
  },

  create(name: string): string {
    const newId = uuidv7();
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
