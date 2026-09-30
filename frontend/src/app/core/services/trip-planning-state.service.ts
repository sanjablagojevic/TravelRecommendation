import { Injectable, signal } from '@angular/core';
import {
  AiPreferenceExtraction,
  ConfirmedTripPreferences,
} from '../models/ai.models';
import { Recommendation } from '../models/recommendation.models';

/**
 * In-memory trip planning state (signals).
 * Free-text AI prompts are not persisted to localStorage.
 */
@Injectable({ providedIn: 'root' })
export class TripPlanningStateService {
  private readonly originalMessageSignal = signal<string | null>(null);
  private readonly extractedSignal = signal<AiPreferenceExtraction | null>(null);
  private readonly confirmedSignal = signal<ConfirmedTripPreferences | null>(null);
  /** Full generate response kept so ScoreBreakdown survives redirect to results. */
  private readonly lastGeneratedSignal = signal<Recommendation | null>(null);

  readonly originalMessage = this.originalMessageSignal.asReadonly();
  readonly extractedPreferences = this.extractedSignal.asReadonly();
  readonly confirmedPreferences = this.confirmedSignal.asReadonly();
  readonly lastGeneratedRecommendation = this.lastGeneratedSignal.asReadonly();

  setExtraction(message: string, extraction: AiPreferenceExtraction): void {
    this.originalMessageSignal.set(message);
    this.extractedSignal.set(extraction);
    this.confirmedSignal.set({
      budget: extraction.budget ?? null,
      budgetCurrency: extraction.budgetCurrency ?? null,
      duration: extraction.duration ?? null,
      tripType: extraction.tripType ?? null,
      travelPeriod: extraction.travelPeriod ?? null,
      preferredClimate: extraction.preferredClimate ?? null,
      interestIds: extraction.interests.map((i) => i.id),
      unmappedPreferences: [...extraction.unmappedPreferences],
      additionalRequirements: extraction.additionalRequirements ?? null,
      requiresCurrencyConversion: extraction.requiresCurrencyConversion,
    });
  }

  updateConfirmed(preferences: ConfirmedTripPreferences): void {
    this.confirmedSignal.set(preferences);
  }

  setLastGenerated(recommendation: Recommendation): void {
    this.lastGeneratedSignal.set(recommendation);
  }

  getLastGenerated(id: number): Recommendation | null {
    const current = this.lastGeneratedSignal();
    return current?.id === id ? current : null;
  }

  clearExtraction(): void {
    this.originalMessageSignal.set(null);
    this.extractedSignal.set(null);
    this.confirmedSignal.set(null);
    this.lastGeneratedSignal.set(null);
  }

  /** Clears all in-memory planning state (call on logout). */
  clearAll(): void {
    this.clearExtraction();
  }

  hasExtraction(): boolean {
    return this.extractedSignal() != null && this.confirmedSignal() != null;
  }
}
