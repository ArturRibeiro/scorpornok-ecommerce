export interface Product {
  id: number;
  name: string;
  sku: string;
  price: number;
  image: string;
  description: string;
}

export interface PageInfo {
  currentPage: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface PagedList<T> {
  pageInfo: PageInfo;
  items: T[];
}
