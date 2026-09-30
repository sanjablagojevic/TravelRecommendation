export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiErrorBody {
  message?: string;
  errors?: Record<string, string[]>;
  detail?: string;
}

export interface DestinationQuery {
  search?: string;
  country?: string;
  city?: string;
  categoryId?: number;
  interestId?: number;
  isActive?: boolean | null;
  includeInactive?: boolean;
  page?: number;
  pageSize?: number;
}
