export interface Paging {
  page: number;
  limit: number;
  totalCount: number;
  totalPages: number;
}

export interface PagedResponse<T> {
  content: T[];
  paging: Paging;
}
