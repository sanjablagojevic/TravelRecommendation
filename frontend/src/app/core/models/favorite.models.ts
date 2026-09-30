export interface FavoriteDestination {
  id: number;
  name: string;
  country: string;
  city: string;
  imageUrl?: string | null;
  averageDailyCost: number;
  climate?: string | null;
}

export interface Favorite {
  id: number;
  addedAt: string;
  destination: FavoriteDestination;
}

export interface FavoriteStatus {
  isFavorite: boolean;
}
