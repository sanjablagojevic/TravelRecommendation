# External APIs

This project integrates third-party services that require API keys. **Never commit real keys** to source control.

## OpenWeather (backend)

Weather data is fetched server-side via the OpenWeather API.

### Configuration

Set the API key using [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development:

```bash
cd src/TravelRecommendation.Api
dotnet user-secrets set "Weather:ApiKey" "YOUR_OPENWEATHER_API_KEY"
dotnet user-secrets set "Weather:Provider" "OpenWeather"
```

Optional settings in `appsettings.json` (no secrets):

```json
"Weather": {
  "Provider": "OpenWeather",
  "BaseUrl": "https://api.openweathermap.org",
  "ApiKey": "",
  "CacheMinutes": 30,
  "TimeoutSeconds": 15
}
```

Weather responses are cached in `ApiCaches` for `CacheMinutes` (default 30).

Do not put the Weather API key in Angular environment files or any frontend bundle.

## Google Maps (frontend)

Interactive maps on the destination detail page use the [Maps JavaScript API](https://developers.google.com/maps/documentation/javascript).

### Configuration

Set `googleMapsApiKey` in `frontend/src/environments/environment.development.ts` for local development only. Keep `environment.ts` empty in the repository and inject production keys via your build or deployment pipeline.

```typescript
export const environment = {
  apiUrl: '...',
  googleMapsApiKey: 'YOUR_BROWSER_API_KEY',
};
```

Create a **Browser key** in [Google Cloud Console](https://console.cloud.google.com/google/maps-apis) with the Maps JavaScript API enabled.

### Key restrictions (recommended)

- **Application restrictions:** HTTP referrers (websites), e.g. `http://localhost:4200/*` for local dev and your production origin(s).
- **API restrictions:** Restrict to **Maps JavaScript API** only.

Do not use a server key or unrestricted keys in the browser. The frontend loads `https://maps.googleapis.com/maps/api/js` directly; JWT auth applies only to the Travel Recommendation API, not Google.
