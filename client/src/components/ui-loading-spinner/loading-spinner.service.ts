import { Injectable, signal } from "@angular/core";

@Injectable({ providedIn: 'root' })
export class LoadingSpinnerService {
  #loading = signal(false);
  loading = this.#loading.asReadonly();

  startLoading(): void {
    this.#loading.set(true);
  }

  stopLoading(): void {
    this.#loading.set(false);
  }
}
