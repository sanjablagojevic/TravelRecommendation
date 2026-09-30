export type ItineraryItemType = 'Attraction' | 'Activity' | 'General';

export interface GenerateItineraryRequest {
  destinationId: number;
  durationDays: number;
  userPreferenceId?: number | null;
  recommendationId?: number | null;
  additionalRequest?: string | null;
}

export interface ItineraryDestinationSummary {
  id: number;
  name: string;
  city: string;
  country: string;
  imageUrl?: string | null;
}

export interface TravelItineraryItem {
  order: number;
  timeOfDay: string;
  title: string;
  description?: string | null;
  itemType: ItineraryItemType;
  attractionId?: number | null;
  attractionName?: string | null;
  attractionImageUrl?: string | null;
  activityId?: number | null;
  activityName?: string | null;
  activityPrice?: number | null;
  activityCurrency?: string | null;
  location?: string | null;
}

export interface TravelItineraryDay {
  dayNumber: number;
  title: string;
  summary?: string | null;
  items: TravelItineraryItem[];
}

export interface TravelItinerary {
  id: number;
  destination: ItineraryDestinationSummary;
  title: string;
  durationDays: number;
  createdAt: string;
  days: TravelItineraryDay[];
}

export interface ItineraryListItem {
  id: number;
  title: string;
  destinationId: number;
  destinationName: string;
  destinationImageUrl?: string | null;
  durationDays: number;
  createdAt: string;
}

export const MIN_ITINERARY_DAYS = 1;
export const MAX_ITINERARY_DAYS = 14;
