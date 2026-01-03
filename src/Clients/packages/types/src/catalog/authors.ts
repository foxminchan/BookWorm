export type Author = {
  id: string;
  name: string | null;
};

export type CreateAuthorRequest = {
  name: string;
};

export type UpdateAuthorRequest = {
  id: string;
  name: string;
};
