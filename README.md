# Travel Recommendation

Hybrid travel recommendation platform for the bachelor thesis project.

Users describe what they want (manually or in natural language), a **deterministic weighted multi-criteria recommendation engine** ranks destinations, and optional OpenAI features assist with preference extraction, explanations, and itinerary generation.

OpenAI **does not** choose destination ranking.

## Architecture

```
Domain ← Application ← Infrastructure ← API
                              ↑
                         Angular SPA
```

- **Domain** — entities and enums (no EF/OpenAI/ASP.NET)
- **Application** — DTOs, interfaces, scoring, validators
- **Infrastructure** — EF Core, JWT, OpenAI, OpenWeather, services
- **API** — ASP.NET Core controllers + middleware
- **Frontend** — Angular + Angular Material

## Technologies

| Layer | Stack |
|-------|--------|
| Backend | ASP.NET Core, C#, Entity Framework Core, Microsoft SQL Server |
| Auth | JWT Bearer, role-based authorization (User / Admin) |
| Frontend | Angular, TypeScript, Angular Material |
| AI | OpenAI API (structured outputs via Responses API) |
| Maps | Google Maps JavaScript API (`@angular/google-maps`) |
| Weather | OpenWeather (server-side only) |

## Main features

- Register / Login / Logout / Profile / Change password
- Destinations (search, filter, pagination, details)
- Favorites
- Manual preference questionnaire
- Natural-language preference extraction (AI) + confirmation
- Deterministic recommendation engine (TOP 5 by default)
- Score breakdown + deterministic explanations + optional AI explanations
- Recommendation history (historical snapshots)
- Weather + Google Maps on destination details
- AI itinerary generator + My Trips
- Admin panel (destinations, categories, interests, attractions, activities)

## Project structure

```
TravelRecommendation/
├── src/
│   ├── TravelRecommendation.Api/
│   ├── TravelRecommendation.Application/
│   ├── TravelRecommendation.Domain/
│   └── TravelRecommendation.Infrastructure/
├── tests/TravelRecommendation.Tests/
├── frontend/                    # Angular SPA
├── docs/EXTERNAL_APIS.md
├── DEMO.md
├── FINAL_PROJECT_REPORT.md
└── README.md
```

## Prerequisites

- .NET 10 SDK
- SQL Server (e.g. LocalDB / SQLEXPRESS)
- Node.js 20+ (for Angular 21)
- Optional: OpenAI API key, OpenWeather API key, Google Maps browser key

## Database setup

```bash
cd src/TravelRecommendation.Api
dotnet ef database update --project ../TravelRecommendation.Infrastructure
```

Development seed creates roles, interests, categories, demo destinations (Barcelona, Rome, Paris, …), attractions and activities. Seed is idempotent for existing destinations/emails.

## Backend setup

```bash
cd src/TravelRecommendation.Api
dotnet user-secrets set "JwtSettings:Key" "YOUR_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_KEY"          # optional
dotnet user-secrets set "Weather:ApiKey" "YOUR_OPENWEATHER_KEY"    # optional
dotnet user-secrets set "SeedAdmin:Email" "admin@example.com"      # optional
dotnet user-secrets set "SeedAdmin:Password" "YOUR_STRONG_PASSWORD" # optional

dotnet run
```

API base (HTTPS): `https://localhost:7103`  
Health: `GET https://localhost:7103/api/health` → `{ "status": "OK" }`

Swagger is available in Development.

See `appsettings.Example.json` for configuration shape (no real secrets).

## Frontend setup

```bash
cd frontend
npm install
# Optional Maps key:
# edit src/environments/environment.development.ts → googleMapsApiKey
npm start
```

App: `http://localhost:4200`  
API URL is configured in `src/environments/environment*.ts`.

## Recommendation engine (deterministic)

Weighted multi-criteria scoring with **dynamic weight normalization**:

| Criterion | Weight |
|-----------|--------|
| Interest | 0.30 |
| Budget | 0.25 |
| Trip type | 0.15 |
| Climate | 0.15 |
| Popularity | 0.15 |

```
FinalScore = sum(activeWeightedScores) / sum(activeWeights)
```

Criteria that the user did not specify are **inactive** (not scored as 0).

- Interest = matched / selected interests  
- Budget: `EstimatedCost = AverageDailyCost × Duration`; if within budget → 1, else `Budget / EstimatedCost`  
- Trip type & climate: deterministic string matching (no LLM)  
- Popularity: clamped 0–1  
- Sort: Score DESC, Popularity DESC, Id ASC  
- Default TOP 5 (1–10 allowed)  
- Stored recommendation items are **historical snapshots** (not recalculated)

## AI usage

Three OpenAI roles only:

1. **Natural-language preference extraction** → structured prefs → user confirmation → engine  
2. **AI-assisted recommendation explanation** (extra layer; deterministic explanation remains)  
3. **AI-generated itinerary** for a selected destination  

OpenAI never sets Score, Rank, or EstimatedCost.

## External APIs

Documented in [docs/EXTERNAL_APIS.md](docs/EXTERNAL_APIS.md):

- Weather key: **server-only** (User Secrets / env)
- Google Maps key: **browser key** with HTTP referrer + API restrictions
- Never put server secrets in the Angular bundle

## Security notes

- JWT validated (issuer, audience, lifetime, signature)
- Public registration always assigns **User** role (no client RoleId)
- Admin mutations require `[Authorize(Roles = "Admin")]`
- Preferences, favorites, recommendations, itineraries are owner-scoped
- PasswordHash never returned by API
- Controllers bind DTOs (not domain entities)
- JWT interceptor attaches Bearer only to `environment.apiUrl`

## Demo

See [DEMO.md](DEMO.md) for a 5–10 minute defence scenario and fallbacks when OpenAI/Weather/Maps are unavailable.

## Final report

See [FINAL_PROJECT_REPORT.md](FINAL_PROJECT_REPORT.md) for architecture, testing, limitations, and future work.

## How to run (quick)

```bash
# Terminal 1 – API
cd src/TravelRecommendation.Api
dotnet run

# Terminal 2 – Angular
cd frontend
npm start
```

Register a user in the UI, or configure `SeedAdmin:*` via User Secrets for an admin account.
