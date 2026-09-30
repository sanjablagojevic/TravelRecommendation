# Final Project Report — Travel Recommendation

## 1. Project Overview

**Travel Recommendation** is a hybrid web application that helps users discover travel destinations matching their preferences and plan short itineraries.

It combines:

- structured user preferences
- a **deterministic recommendation engine** (weighted multi-criteria scoring with **dynamic weight normalization**)
- **natural-language preference extraction** (LLM)
- **AI-assisted explanations**
- **AI-generated itineraries**

It is **not** a machine-learning recommendation model or neural ranking system.

## 2. Architecture

Layered backend:

`Domain → Application → Infrastructure → API`

Angular SPA consumes the REST API with JWT authentication.

External providers (OpenAI, OpenWeather, Google Maps) enrich UX; they do not replace core ranking logic.

## 3. Backend

- ASP.NET Core Web API (.NET 10)
- Entity Framework Core + SQL Server
- JWT authentication and role authorization
- Application services for destinations, preferences, favorites, recommendations, itineraries, weather, admin dashboard
- Global exception middleware mapping `AppException` to safe HTTP responses

## 4. Frontend

- Angular 21 + TypeScript + Angular Material
- Standalone components, lazy routes
- Auth/admin/guest guards
- JWT interceptor scoped to backend `apiUrl` only

## 5. Database

Major entities include Users/Roles, Destinations (categories, interests, attractions, activities), UserPreferences, Favorites, Recommendations/RecommendationItems, ApiCache, TravelItineraries/Days/Items.

Migrations are additive (no squash). Soft-delete for destinations (`IsActive`). Hard delete for user itineraries.

Money and scores use `decimal` (not float).

## 6. Authentication and Authorization

- Register/Login issue JWT; `/api/auth/me` restores session after refresh
- Public registration always assigns **User** (no Role mass-assignment)
- Admin mutations require `[Authorize(Roles = "Admin")]`
- Preferences, favorites, recommendations, itineraries are owner-scoped (foreign access → 404)
- PasswordHash never exposed in DTOs
- Demo admin created only when `SeedAdmin:Email` / `SeedAdmin:Password` are configured via User Secrets

## 7. Recommendation Engine

Weights (sum = 1.00):

| Criterion | Weight |
|-----------|--------|
| Interest | 0.30 |
| Budget | 0.25 |
| TripType | 0.15 |
| Climate | 0.15 |
| Popularity | 0.15 |

**Dynamic normalization:** inactive criteria are excluded from numerator and denominator.

Example (Interest/Budget/Popularity active; TripType & Climate inactive):

- InterestScore = 0.75, BudgetScore = 1.0, PopularityScore = 0.80  
- Final = (0.30×0.75 + 0.25×1.0 + 0.15×0.80) / (0.30+0.25+0.15) ≈ **0.7786**

Tie-break: Score DESC → Popularity DESC → Id ASC.

Default TopCount = 5. Historical items store Score/Rank/EstimatedCost/Explanation as snapshots.

## 8. AI Integration

| Function | Role |
|----------|------|
| Preference extraction | Free text → structured prefs → confirmation |
| Recommendation explanation | Extra narrative; does not change scores |
| Itinerary generation | Day-by-day plan for a **selected** destination |

OpenAI never sets Score, Rank, or EstimatedCost. Non-EUR currencies require user confirmation (no fake FX conversion).

## 9. Weather and Maps

- Weather: Angular → API → `IWeatherService` → OpenWeather; cached in `ApiCache` (~30 min); coordinate changes invalidate cache
- Maps: browser Maps JavaScript API; graceful “unavailable” without key
- Weather is **not** used for recommendation scoring or future itinerary climate planning

## 10. Itinerary Generator

Structured persistence: TravelItinerary → Days → Items (`Attraction` / `Activity` / `General`).

Backend validates day count, day numbers, and AttractionId/ActivityId ownership for the destination. Invalid AI output is rejected (no empty save). Owner-only GET/DELETE.

## 11. Admin Panel

Admin layout with dashboard counts and CRUD for destinations (soft deactivate), categories, interests, attractions, activities. Attractions/activities managed in destination edit tabs.

## 12. Testing

Backend unit tests cover scoring (interest, budget, normalization, ties), auth/ownership patterns, weather cache, itinerary validation/ownership, AI mapping guards, and related services.

Frontend uses `ng test` (Karma) when configured; production verification uses `npm run build`.

## 13. Security

- No real API keys in repository (`appsettings` placeholders + User Secrets)
- JWT validation enabled; tokens not logged
- DTO binding; server-owned fields not client-writable
- Safe error responses (no stack traces to clients in production path)
- External keys stay on backend (except restricted browser Maps key)

## 14. Known Limitations

- Destination costs and popularity are demo/estimated data
- Activities are local catalog data (no live booking)
- No hotel/flight booking or payment
- No real-time currency conversion API
- Weather is short-term provider data, not seasonal climate for trip months
- AI output depends on OpenAI availability and quality
- Google Maps requires a configured, restricted browser key
- No PDF export / sharing / collaborative filtering

## 15. Future Work

- Live hotel / flight APIs and booking integration
- Seasonal climate datasets
- Public transport / itinerary routing (e.g. Google Routes)
- Collaborative filtering or ML ranking experiments (separate from current engine)
- Larger destination dataset
- PDF itinerary export and sharing
- Optional external activity providers (e.g. GetYourGuide-style APIs) — **not implemented** in this project

## 16. How to Run

See [README.md](README.md).

## 17. Demo Scenario

See [DEMO.md](DEMO.md).
