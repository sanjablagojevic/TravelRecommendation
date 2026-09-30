export interface CurrentUser {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: CurrentUser;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface StoredAuth {
  token: string;
  expiresAt: string;
  user: CurrentUser;
}
