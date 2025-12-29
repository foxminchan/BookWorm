export type PagedResult<T> = {
  items: T[];
  totalCount: number;
  link?: string;
};
