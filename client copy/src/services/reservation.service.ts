import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IReservation } from '../models/reservation';
import { Subject } from 'rxjs';
import { UserService } from './user.service';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class ReservationService {
  private baseUrl = `${environment.baseUrl}Reservations`;
  private reservationsUpdated = new Subject<void>(); // Subject to notify about new reservations

  reservationsUpdated$ = this.reservationsUpdated.asObservable(); // Observable that components can subscribe to

	constructor(
		private _httpClient: HttpClient,
		private _userService: UserService,
	) { }

	getAllReservations(): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<IReservation[]>(this.baseUrl).subscribe({
				next: result => {
					console.debug(`getReservations: url ${this.baseUrl} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`getReservations: url ${this.baseUrl}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationsByBoatId(boatId: string): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<IReservation[]>(`${this.baseUrl}/boat/${boatId}`).subscribe({
				next: result => {
					console.debug(`getReservationById: url ${this.baseUrl}/boat/${boatId} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`getReservationById: url ${this.baseUrl}/boat/${boatId}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationsByUserId(userId: string): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<IReservation[]>(`${this.baseUrl}/user/${userId}`).subscribe({
				next: result => {
					console.debug(`getReservationById: url ${this.baseUrl}/user/${userId} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`getReservationById: url ${this.baseUrl}/user/${userId}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationByDateAndBoatId(day: number, month: number, year: number, boatId: string): Promise<IReservation> {
		return new Promise((resolve, reject) => {
			// Create a query URL with date parameters
			const url = `${this.baseUrl}/by-date-and-boatId?day=${day}&month=${month}&year=${year}&boatId=${boatId}`;
	
			this._httpClient.get<IReservation>(url).subscribe({
				next: result => {
					console.debug(`getReservationByDate: url ${url} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`getReservationByDate: url ${url}`, err);
					reject(err);
				}
			});
		});
	}
	

	createReservation(reservation: IReservation): Promise<string> {
		return new Promise((resolve, reject) => {
			this._httpClient.post<string>(`${this.baseUrl}/add`, reservation, { responseType: 'text' as 'json' }).subscribe({
				next: result => {
					console.debug(`createReservation: url ${this.baseUrl} result`, result);
          this.reservationsUpdated.next(); // Notify subscribers
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`createReservation: url ${this.baseUrl}`, err);
					reject(err);
				}
			});
		});
	}

	confirmReservation(reservation: IReservation): Promise<IReservation> {
		return new Promise((resolve, reject) => {
			this._httpClient.put<IReservation>(`${this.baseUrl}/confirm-reservation`, reservation, {}).subscribe({
				next: result => {
					console.debug(`confirmReservation: url ${this.baseUrl}/confirm/${reservation.reservationId} result`, result);
					this.reservationsUpdated.next(); // Notify subscribers
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`confirmReservation: url ${this.baseUrl}/confirm/${reservation.reservationId}`, err);
					reject(err);
				}
			});
		});
	}
	
	getOccupiedDatesForYear(year: number): Promise<Date[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<IReservation[]>(`${this.baseUrl}/occupied/year/${year}`).subscribe({
				// Transform the response to an array of Date objects
				next: result => {
					console.debug(`getOccupiedDatesForYear: url ${this.baseUrl}/occupied/year/${year} result`, result);
	
					// Convert each reservation to a JavaScript Date object
					const dates = result.map(reservation => {
						// Create a new Date object for each reservation
						return new Date(reservation.year, reservation.month - 1, reservation.day); // Note: month is 0-indexed in JS Date
					});
	
					resolve(dates); // Resolve with the array of Date objects
				},
				error: err => {
					console.warn(`getOccupiedDatesForYear: url ${this.baseUrl}/occupied/year/${year}`, err);
					reject(err); // Reject if an error occurs
				}
			});
		});
	}
	
	

	deleteReservationById(id: string): Promise<IReservation> {
		return new Promise((resolve, reject) => {
			this._httpClient.delete<IReservation>(`${this.baseUrl}/${id}`).subscribe({
				next: result => {
					console.debug(`deleteReservationById: url ${this.baseUrl}/${id} result`, result);
          this.reservationsUpdated.next();
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`deleteReservationById: url ${this.baseUrl}/${id}`, err);
					reject(err);
				}
			});
		});
	}

	deletePastReservations(): Promise<void> {
		return new Promise((resolve, reject) => {
			this._httpClient.delete<void>(`${this.baseUrl}/delete-past`).subscribe({
				next: result => {
					console.debug(`deletePastReservations: url ${this.baseUrl}/delete-past result`, result);
					this.reservationsUpdated.next();
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`deletePastReservations: url ${this.baseUrl}/delete-past`, err);
					reject(err);
				}
			});
		});
	}

	updateReservations(): Promise<void> {
		return new Promise((resolve, reject) => {
			this._httpClient.put<void>(`${this.baseUrl}/update-reservations`, {}).subscribe({
				next: result => {
					console.debug(`updateReservations: url ${this.baseUrl}/update result`, result);
					this.reservationsUpdated.next();
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`updateReservations: url ${this.baseUrl}/update`, err);
					reject(err);
				}
			});
		});
	}
}