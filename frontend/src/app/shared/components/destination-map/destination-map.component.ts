import {
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild,
} from '@angular/core';
import {
  GoogleMap,
  MapInfoWindow,
  MapMarker,
} from '@angular/google-maps';
import { GoogleMapsConfigService } from '../../../core/services/google-maps-config.service';

export interface DestinationMapAttraction {
  name: string;
  location?: string | null;
  latitude?: number | null;
  longitude?: number | null;
  description?: string | null;
}

interface MapMarkerEntry {
  id: string;
  kind: 'destination' | 'attraction';
  title: string;
  subtitle?: string | null;
  description?: string | null;
  position: google.maps.LatLngLiteral;
}

function isValidCoord(
  latitude: number | null | undefined,
  longitude: number | null | undefined
): boolean {
  if (latitude == null || longitude == null) {
    return false;
  }
  if (!Number.isFinite(latitude) || !Number.isFinite(longitude)) {
    return false;
  }
  if (latitude === 0 && longitude === 0) {
    return false;
  }
  return true;
}

@Component({
  selector: 'app-destination-map',
  imports: [GoogleMap, MapMarker, MapInfoWindow],
  template: `
    <div class="map-shell">
      @if (mapLoading()) {
        <p class="muted">Loading map…</p>
      } @else if (mapUnavailable() || markers().length === 0) {
        <p class="muted">Map is currently unavailable.</p>
      } @else {
        <google-map
          height="320px"
          width="100%"
          [center]="defaultCenter"
          [zoom]="defaultZoom"
          (mapInitialized)="onMapInitialized($event)"
        >
          @for (marker of markers(); track marker.id) {
            <map-marker
              #markerRef="mapMarker"
              [position]="marker.position"
              [title]="marker.title"
              [options]="markerOptions(marker.kind)"
              (mapClick)="openInfo(markerRef, marker)"
            />
          }
          <map-info-window>
            @if (activeMarker(); as active) {
              <div class="info-window">
                <strong>{{ active.title }}</strong>
                @if (active.subtitle) {
                  <p class="info-sub">{{ active.subtitle }}</p>
                }
                @if (active.description) {
                  <p>{{ active.description }}</p>
                }
              </div>
            }
          </map-info-window>
        </google-map>
      }
    </div>
  `,
  styles: `
    .map-shell {
      width: 100%;
      min-height: 320px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 8px;
      overflow: hidden;
      background: var(--mat-sys-surface-container-low);
    }

    google-map {
      display: block;
      width: 100%;
    }

    .muted {
      color: var(--mat-sys-on-surface-variant);
      margin: 0;
      padding: 1rem;
      text-align: center;
    }

    .info-window {
      max-width: 220px;
      font-size: 0.875rem;
      line-height: 1.35;
    }

    .info-window p {
      margin: 0.35rem 0 0;
    }

    .info-sub {
      color: #555;
    }
  `,
})
export class DestinationMapComponent {
  private readonly mapsConfig = inject(GoogleMapsConfigService);
  private mapInstance: google.maps.Map | null = null;

  readonly destinationName = input.required<string>();
  readonly city = input<string>('');
  readonly country = input<string>('');
  readonly latitude = input<number | null | undefined>(null);
  readonly longitude = input<number | null | undefined>(null);
  readonly attractions = input<DestinationMapAttraction[]>([]);

  readonly mapLoading = signal(true);
  readonly mapUnavailable = signal(false);
  readonly activeMarker = signal<MapMarkerEntry | null>(null);

  readonly infoWindow = viewChild(MapInfoWindow);

  readonly defaultCenter: google.maps.LatLngLiteral = { lat: 0, lng: 0 };
  readonly defaultZoom = 2;

  readonly markers = computed(() => {
    const entries: MapMarkerEntry[] = [];
    const destLat = this.latitude();
    const destLng = this.longitude();

    if (isValidCoord(destLat, destLng)) {
      entries.push({
        id: 'destination',
        kind: 'destination',
        title: this.destinationName(),
        subtitle: [this.city(), this.country()].filter(Boolean).join(', ') || null,
        position: { lat: destLat!, lng: destLng! },
      });
    }

    this.attractions().forEach((attraction, index) => {
      if (!isValidCoord(attraction.latitude, attraction.longitude)) {
        return;
      }
      entries.push({
        id: `attraction-${index}`,
        kind: 'attraction',
        title: attraction.name,
        subtitle: attraction.location ?? null,
        description: attraction.description ?? null,
        position: {
          lat: attraction.latitude!,
          lng: attraction.longitude!,
        },
      });
    });

    return entries;
  });

  constructor() {
    void this.initializeMap();

    effect(() => {
      const map = this.mapInstance;
      const markerList = this.markers();
      if (map && markerList.length > 0) {
        this.fitMapBounds(map, markerList);
      }
    });
  }

  markerOptions(kind: MapMarkerEntry['kind']): google.maps.MarkerOptions {
    if (kind === 'destination') {
      return {};
    }
    return { opacity: 0.92 };
  }

  onMapInitialized(map: google.maps.Map): void {
    this.mapInstance = map;
    this.fitMapBounds(map, this.markers());
  }

  openInfo(markerRef: MapMarker, marker: MapMarkerEntry): void {
    this.activeMarker.set(marker);
    this.infoWindow()?.open(markerRef);
  }

  private async initializeMap(): Promise<void> {
    if (!this.mapsConfig.hasApiKey()) {
      this.mapUnavailable.set(true);
      this.mapLoading.set(false);
      return;
    }

    const loaded = await this.mapsConfig.loadApi();
    this.mapLoading.set(false);
    if (!loaded) {
      this.mapUnavailable.set(true);
    }
  }

  private fitMapBounds(map: google.maps.Map, markerList: MapMarkerEntry[]): void {
    if (markerList.length === 0) {
      return;
    }
    if (markerList.length === 1) {
      map.setCenter(markerList[0].position);
      map.setZoom(12);
      return;
    }

    const bounds = new google.maps.LatLngBounds();
    markerList.forEach((marker) => bounds.extend(marker.position));
    map.fitBounds(bounds, 48);
  }
}
