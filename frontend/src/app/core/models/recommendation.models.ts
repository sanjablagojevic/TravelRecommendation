export interface GenerateRecommendationRequest {
  userPreferenceId: number;
  topCount?: number | null;
}

export interface RecommendationDestination {
  id: number;
  name: string;
  country: string;
  city: string;
  imageUrl?: string | null;
  averageDailyCost: number;
  climate?: string | null;
}

export interface ScoreComponent {
  name: string;
  score?: number | null;
  weight: number;
  isActive: boolean;
  weightedValue?: number | null;
}

export interface ScoreBreakdown {
  interestScore?: number | null;
  budgetScore?: number | null;
  tripTypeScore?: number | null;
  climateScore?: number | null;
  popularityScore?: number | null;
  activeWeightTotal: number;
  components: ScoreComponent[];
}

export interface RecommendationItem {
  rank: number;
  destination: RecommendationDestination;
  score: number;
  matchPercentage: number;
  estimatedCost?: number | null;
  matchedInterests: string[];
  missingInterests: string[];
  explanation: string;
  aiExplanation?: string | null;
  scoreBreakdown?: ScoreBreakdown | null;
}

export interface Recommendation {
  id: number;
  createdAt: string;
  userPreferenceId: number;
  items: RecommendationItem[];
}

export interface RecommendationHistoryItem {
  id: number;
  createdAt: string;
  userPreferenceId: number;
  topDestination?: string | null;
  topScore?: number | null;
  numberOfResults: number;
}

export interface AiExplanationItem {
  recommendationItemId: number;
  destinationId: number;
  rank: number;
  score: number;
  estimatedCost?: number | null;
  deterministicExplanation?: string | null;
  aiExplanation?: string | null;
}

export interface AiExplanationsResponse {
  recommendationId: number;
  usedFallback: boolean;
  warning?: string | null;
  items: AiExplanationItem[];
}
