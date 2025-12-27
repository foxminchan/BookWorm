export type CategoryDto = {
  id: string;
  name: string | null;
};

export type CreateCategoryRequest = {
  name: string;
};

export type UpdateCategoryRequest = {
  id: string;
  name: string;
};
