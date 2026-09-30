export interface CreateUserPreferenceRequest {
  budget?: number | null;
  duration?: number | null;
  tripType?: string | null;
  travelPeriod?: string | null;
  preferredClimate?: string | null;
  minTemperature?: number | null;
  maxTemperature?: number | null;
  interestIds: number[];
}

export interface PreferenceInterest {
  id: number;
  name: string;
}

export interface UserPreference {
  id: number;
  budget?: number | null;
  duration?: number | null;
  tripType?: string | null;
  travelPeriod?: string | null;
  preferredClimate?: string | null;
  minTemperature?: number | null;
  maxTemperature?: number | null;
  createdAt: string;
  interests: PreferenceInterest[];
}

export const SUPPORTED_TRIP_TYPES = [
  'Beach',
  'City Break',
  'Nature',
  'Culture',
  'Adventure',
  'Relaxation',
  'Mountains',
] as const;

export const TRAVEL_PERIODS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
] as const;

/** Climate values that exist in the demo destination catalog. */
export const DEMO_CLIMATES = [
  'Mediterranean',
  'Temperate',
  'Continental',
  'Subtropical',
  'Semi-arid',
] as const;
