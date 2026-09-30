# Demo Scenario (~5–10 minutes)

Use this script for the bachelor thesis defence. Prefer a prepared demo user account and a working backend.

## Before you start

1. Backend running (`dotnet run` in `src/TravelRecommendation.Api`)
2. Frontend running (`npm start` in `frontend`)
3. Optional keys: OpenAI, Weather, Google Maps (see README)
4. Register a demo user **or** use SeedAdmin credentials from User Secrets

### Fallbacks if providers are unavailable

| Feature | Fallback |
|---------|----------|
| OpenAI preference extraction | Use **Manual** questionnaire |
| OpenAI explanations / itinerary | Show saved history / skip AI steps |
| Weather | Destination page still works; weather shows unavailable |
| Google Maps | Destination page still works; map shows unavailable |
| Recommendation Engine | Always works offline (deterministic, no OpenAI) |

---

## Step 1 — Home (~30 s)

Open `/`. Show destinations and responsive layout.

## Step 2 — Login (~30 s)

Login as a normal **User**.

## Step 3 — Plan a Trip → AI (~1 min)

Go to **Plan a Trip** → **AI**.

Prompt example:

> I have 1000 EUR for 5 days. I like beaches, culture and food. I would like a relaxing trip.

*(If OpenAI is down: use Manual questionnaire with the same values.)*

## Step 4 — AI extraction + confirmation (~1 min)

Show structured preferences. Emphasize:

- NLP turns free text into structured data
- User can **edit** before saving
- This is **not** destination ranking

Confirm → save preference.

## Step 5 — Recommendations (~1 min)

Generate recommendations. Show **TOP 5**.

Emphasize:

- Ranking comes from the **deterministic recommendation engine**
- OpenAI did **not** pick the order

## Step 6 — Score breakdown (~1 min)

Open **Why this match?** / score breakdown.

Show Interest, Budget, Trip Type, Climate, Popularity.  
Mention **dynamic weight normalization** (unspecified criteria are inactive, not scored as 0%).

## Step 7 — Destination details (~1 min)

Open a top destination (e.g. Barcelona).

Show About, interests, attractions, activities, Weather, Google Maps.

## Step 8 — Favorites (~30 s)

Add to favorites → open **Favorites**.

## Step 9 — AI itinerary (~1–2 min)

From the recommendation result or destination details: **Plan this trip** / **Create itinerary**.

Choose duration (e.g. 4 days) → Generate.

Show Day 1…N timeline.

Emphasize difference:

| System | Answers |
|--------|---------|
| Recommendation Engine | *Where should I go?* |
| AI Itinerary Generator | *What should I do there?* |

## Step 10 — History / My Trips (~30 s)

Briefly open **History** and **My Trips**.

## Step 11 — Admin (optional, short) (~1 min)

Login as Admin (SeedAdmin). Show Dashboard and one Destination edit / attractions tab.  
Do **not** spend half the presentation on CRUD.

---

## Talking points for the board

1. Hybrid system: structured preferences + deterministic scoring + LLM assistance  
2. Not a machine-learning ranker  
3. Historical recommendations are snapshots  
4. External APIs enrich display; they do not replace the scoring engine  
5. Honest limitations: demo costs, no booking, no live FX conversion  
