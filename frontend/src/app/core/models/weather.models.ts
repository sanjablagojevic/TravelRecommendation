export interface CurrentWeather {
  temperature: number;
  feelsLike: number;
  minTemperature?: number | null;
  maxTemperature?: number | null;
  humidity: number;
  description: string;
  icon?: string | null;
  windSpeed: number;
  observedAt: string;
}

export interface ForecastDay {
  date: string;
  minTemperature: number;
  maxTemperature: number;
  description: string;
  icon?: string | null;
}

export interface WeatherResponse {
  destinationId: number;
  destinationName: string;
  current: CurrentWeather;
  forecast: ForecastDay[];
  fetchedAt: string;
  isCached: boolean;
  isStale: boolean;
}
