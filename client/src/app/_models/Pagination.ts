export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalPages: number;
  totalItems: number;
}

export class PaginatedResult<T> {
  Result: T;
  Pagination: Pagination;
}
