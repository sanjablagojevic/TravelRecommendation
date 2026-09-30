import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

const MAPS_SCRIPT_SELECTOR = 'script[src*="maps.googleapis.com/maps/api/js"]';

@Injectable({ providedIn: 'root' })
export class GoogleMapsConfigService {
  private loadPromise: Promise<boolean> | null = null;

  hasApiKey(): boolean {
    return environment.googleMapsApiKey.trim().length > 0;
  }

  loadApi(): Promise<boolean> {
    if (!this.hasApiKey()) {
      return Promise.resolve(false);
    }

    if (this.loadPromise) {
      return this.loadPromise;
    }

    this.loadPromise = this.injectScript();
    return this.loadPromise;
  }

  private injectScript(): Promise<boolean> {
    if (typeof document === 'undefined') {
      return Promise.resolve(false);
    }

    const existing = document.querySelector(MAPS_SCRIPT_SELECTOR);
    if (existing) {
      return this.waitForGoogleMaps();
    }

    return new Promise((resolve) => {
      const script = document.createElement('script');
      const key = encodeURIComponent(environment.googleMapsApiKey.trim());
      script.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
      script.async = true;
      script.defer = true;
      script.onload = () => {
        void this.waitForGoogleMaps().then(resolve);
      };
      script.onerror = () => resolve(false);
      document.head.appendChild(script);
    });
  }

  private waitForGoogleMaps(): Promise<boolean> {
    if (typeof window !== 'undefined' && window.google?.maps) {
      return Promise.resolve(true);
    }

    return new Promise((resolve) => {
      let attempts = 0;
      const maxAttempts = 50;
      const interval = window.setInterval(() => {
        attempts++;
        if (window.google?.maps) {
          window.clearInterval(interval);
          resolve(true);
        } else if (attempts >= maxAttempts) {
          window.clearInterval(interval);
          resolve(false);
        }
      }, 100);
    });
  }
}
