export type Buyer = {
  id: string;
  name: string | null;
  address: string | null;
};

export type CreateBuyerRequest = {
  street: string;
  city: string;
  province: string;
};

export type UpdateAddressRequest = {
  street: string;
  city: string;
  province: string;
};

export type ListBuyersQuery = {
  pageIndex?: number;
  pageSize?: number;
};
