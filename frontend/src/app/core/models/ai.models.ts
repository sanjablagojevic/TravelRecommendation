export interface AiPreferenceRequest {
  message: string;
}

export interface AiMappedInterest {
  id: number;
  name: string;
}

export interface AiPreferenceExtraction {
  budget?: number | null;
  budgetCurrency?: string | null;
  duration?: number | null;
  tripType?: string | null;
  travelPeriod?: string | null;
  preferredClimate?: string | null;
  minTemperature?: number | null;
  maxTemperature?: number | null;
  interests: AiMappedInterest[];
  unmappedPreferences: string[];
  additionalRequirements?: string | null;
  requiresCurrencyConversion: boolean;
  currencyNote?: string | null;
}

export interface CreatePreferenceFromAiRequest {
  budget?: number | null;
  budgetCurrency?: string | null;
  duration?: number | null;
  tripType?: string | null;
  travelPeriod?: string | null;
  preferredClimate?: string | null;
  minTemperature?: number | null;
  maxTemperature?: number | null;
  interestIds: number[];
}

export interface CreatePreferenceFromAiResponse {
  userPreferenceId: number;
}

/** Editable confirmation form model after AI extraction. */
export interface ConfirmedTripPreferences {
  budget?: number | null;
  budgetCurrency?: string | null;
  duration?: number | null;
  tripType?: string | null;
  travelPeriod?: string | null;
  preferredClimate?: string | null;
  interestIds: number[];
  unmappedPreferences: string[];
  additionalRequirements?: string | null;
  requiresCurrencyConversion: boolean;
}
