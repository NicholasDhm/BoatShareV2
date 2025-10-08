import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IBoat } from '../models/boat';
import { BehaviorSubject } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BoatService {
  private baseUrl = `${environment.baseUrl}Boats`;

  private _boatsSubject = new BehaviorSubject<IBoat[]>([]);
  public boats$ = this._boatsSubject.asObservable();

  constructor(
    private _httpClient: HttpClient
  ) {}

  // Method to fetch boats from the API (public - for registration)
  fetchBoats(): void {
    this._httpClient.get<IBoat[]>(`${this.baseUrl}/public`).subscribe({
      next: boats => {
        this._boatsSubject.next(boats); // Update the BehaviorSubject
      },
      error: err => {
        console.error('Failed to fetch boats', err);
      }
    });
  }

  getAllBoats(): Promise<IBoat[]> {
    return new Promise((resolve, reject) => {
      this._httpClient.get<IBoat[]>(this.baseUrl).subscribe({
        next: result => {
          console.debug(`getBoats: url ${this.baseUrl} result`, result);
          this._boatsSubject.next(result);
          resolve(result);
        },
        error: err => {
          console.warn(`getBoats: url ${this.baseUrl}`, err);
          reject(err);
        }
      });
    });
  }
  
  getBoatByBoatId(boatId: number): Promise<IBoat> {
    return new Promise((resolve, reject) => {
      this._httpClient.get<IBoat>(`${this.baseUrl}/${boatId}`).subscribe({
        next: result => {
          console.debug(`getBoatById: url ${this.baseUrl}/${boatId} result`, result);
          resolve(result);
        },
        error: err => {
          console.warn(`getBoatById: url ${this.baseUrl}/${boatId}`, err);
          reject(err);
        }
      });
    });
  }

  postBoat(boat: IBoat): Promise<IBoat> {
    return new Promise((resolve, reject) => {
      this._httpClient.post<IBoat>(this.baseUrl, boat).subscribe({
        next: result => {
          console.debug(`postUser: url ${this.baseUrl} result`, result);

          const currentBoats = this._boatsSubject.getValue();
          this._boatsSubject.next([...currentBoats, result]);

          resolve(result);
        },
        error: err => {
          console.warn(`postUser: url ${this.baseUrl}`, err);
          reject(err);
        }
      });
    });
  }

  deleteBoatById(id: number): Promise<void> {
    return new Promise((resolve, reject) => {
      this._httpClient.delete(`${this.baseUrl}/${id}`).subscribe({
        next: () => {
          console.debug(`deleteBoatById: url ${this.baseUrl}/${id} succeeded`);

          const currentBoats = this._boatsSubject.getValue().filter(boat => boat.boatId !== id);
          this._boatsSubject.next(currentBoats); // Update the BehaviorSubject

          resolve(); // Resolve with no value since we don't return the deleted boat
        },
        error: err => {
          console.warn(`deleteBoatById: url ${this.baseUrl}/${id}`, err);
          reject(err); // Reject if there's an error
        }
      });
    });
  }
  
}