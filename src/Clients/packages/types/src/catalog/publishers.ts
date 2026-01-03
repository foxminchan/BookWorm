export type Publisher = {
  id: string;
  name: string | null;
};

export type CreatePublisherRequest = {
  name: string;
};

export type UpdatePublisherRequest = {
  id: string;
  name: string;
};
