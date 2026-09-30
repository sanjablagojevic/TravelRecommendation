import { Category } from './category.models';

export interface DestinationInterestRef {
  id: number;
  name: string;
}

export interface DestinationListItem {
  id: number;
  name: string;
  country: string;
  city: string;
  averageDailyCost: number;
  popularity: number;
  imageUrl?: string | null;
  climate?: string | null;
  isActive: boolean;
  categories: Category[];
  interests: DestinationInterestRef[];
}

export interface Attraction {
  id: number;
  destinationId: number;
  name: string;
  description?: string | null;
  location?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  imageUrl?: string | null;
}

export interface Activity {
  id: number;
  destinationId: number;
  name: string;
  provider?: string | null;
  price?: number | null;
  currency?: string | null;
  externalUrl?: string | null;
  description?: string | null;
}

export interface DestinationDetails extends DestinationListItem {
  description?: string | null;
  latitude: number;
  longitude: number;
  createdAt: string;
  attractions: Attraction[];
  activities: Activity[];
}

export interface CreateDestinationRequest {
  name: string;
  country: string;
  city: string;
  description?: string | null;
  averageDailyCost: number;
  latitude: number;
  longitude: number;
  popularity: number;
  imageUrl?: string | null;
  climate?: string | null;
  isActive: boolean;
  categoryIds: number[];
  interestIds: number[];
}

export type UpdateDestinationRequest = CreateDestinationRequest;

export interface CreateAttractionRequest {
  destinationId: number;
  name: string;
  description?: string | null;
  location?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  imageUrl?: string | null;
}

export type UpdateAttractionRequest = Omit<CreateAttractionRequest, 'destinationId'>;

export interface CreateActivityRequest {
  destinationId: number;
  name: string;
  provider?: string | null;
  price?: number | null;
  currency?: string | null;
  externalUrl?: string | null;
  description?: string | null;
}

export type UpdateActivityRequest = Omit<CreateActivityRequest, 'destinationId'>;
