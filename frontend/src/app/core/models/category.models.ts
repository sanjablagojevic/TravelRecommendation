export interface Category {
  id: number;
  name: string;
  type: string;
  description?: string | null;
}

export interface Interest {
  id: number;
  name: string;
  description?: string | null;
}

export interface CreateCategoryRequest {
  name: string;
  type: string;
  description?: string | null;
}

export type UpdateCategoryRequest = CreateCategoryRequest;

export interface CreateInterestRequest {
  name: string;
  description?: string | null;
}

export type UpdateInterestRequest = CreateInterestRequest;
