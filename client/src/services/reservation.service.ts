import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IReservation } from '../models/reservation';
import { Subject } from 'rxjs';
import { UserService } from './user.service';
import { environment } from '../environments/environment';

interface ReservationResponseDTO {
	reservationId: number;
	userId: number;
	userName?: string;
	boatId: number;
	boatName?: string;
	startTime: string;
	endTime: string;
	durationHours: number;
	totalCost: number;
	status: 'Pending' | 'Confirmed' | 'Unconfirmed' | 'Cancelled' | 'Legacy';
	reservationType: 'Standard' | 'Substitution' | 'Contingency';
	notes: string;
	createdAt: string;
	updatedAt: string;
}

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

	private mapResponseToReservation(dto: ReservationResponseDTO): IReservation {
		const startDate = new Date(dto.startTime);
		return {
			reservationId: dto.reservationId,
			status: dto.status,
			createdAtIsoDate: dto.createdAt,
			userName: dto.userName,
			type: dto.reservationType,
			userId: dto.userId,
			boatId: dto.boatId,
			year: startDate.getFullYear(),
			month: startDate.getMonth() + 1, // JS months are 0-indexed
			day: startDate.getDate()
		};
	}

	getAllReservations(): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<ReservationResponseDTO[]>(this.baseUrl).subscribe({
				next: result => {
					console.debug(`getReservations: url ${this.baseUrl} result`, result);
					const mappedReservations = result.map(dto => this.mapResponseToReservation(dto));
					resolve(mappedReservations);
				},
				error: err => {
					console.warn(`getReservations: url ${this.baseUrl}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationsByBoatId(boatId: number): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<ReservationResponseDTO[]>(`${this.baseUrl}/boat/${boatId}`).subscribe({
				next: result => {
					console.debug(`getReservationById: url ${this.baseUrl}/boat/${boatId} result`, result);
					const mappedReservations = result.map(dto => this.mapResponseToReservation(dto));
					resolve(mappedReservations);
				},
				error: err => {
					console.warn(`getReservationById: url ${this.baseUrl}/boat/${boatId}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationsByUserId(userId: number): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<ReservationResponseDTO[]>(`${this.baseUrl}/user/${userId}`).subscribe({
				next: result => {
					console.debug(`getReservationById: url ${this.baseUrl}/user/${userId} result`, result);
					const mappedReservations = result.map(dto => this.mapResponseToReservation(dto));
					resolve(mappedReservations);
				},
				error: err => {
					console.warn(`getReservationById: url ${this.baseUrl}/user/${userId}`, err);
					reject(err);
				}
			});
		});
	}

	getReservationByDateAndBoatId(day: number, month: number, year: number, boatId: number): Promise<IReservation> {
		return new Promise((resolve, reject) => {
			// Create a query URL with date parameters
			const url = `${this.baseUrl}/by-date-and-boatId?day=${day}&month=${month}&year=${year}&boatId=${boatId}`;

			this._httpClient.get<ReservationResponseDTO>(url).subscribe({
				next: result => {
					console.debug(`getReservationByDate: url ${url} result`, result);
					const mappedReservation = this.mapResponseToReservation(result);
					resolve(mappedReservation);
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
			// Transform IReservation to CreateReservationDTO format
			// Format dates as local datetime strings (YYYY-MM-DDTHH:mm:ss) without timezone
			// Backend will interpret these as Brazil time
			const createReservationDto = {
				boatId: reservation.boatId,
				startTime: this.formatLocalDateTime(reservation.year, reservation.month, reservation.day, 6, 0, 0),
				endTime: this.formatLocalDateTime(reservation.year, reservation.month, reservation.day, 18, 0, 0),
				reservationType: reservation.type,
				notes: ''
			};

			this._httpClient.post<string>(`${this.baseUrl}`, createReservationDto, { responseType: 'text' as 'json' }).subscribe({
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
			if (!reservation.reservationId) {
				reject(new Error('Reservation ID is required'));
				return;
			}

			const reservationDto = {
				reservationId: reservation.reservationId,
				userId: reservation.userId,
				boatId: reservation.boatId,
				startTime: this.formatLocalDateTime(reservation.year, reservation.month, reservation.day, 6, 0, 0),
				endTime: this.formatLocalDateTime(reservation.year, reservation.month, reservation.day, 18, 0, 0),
				reservationType: reservation.type,
				status: 'Confirmed',
				notes: ''
			};

			this._httpClient.put<ReservationResponseDTO>(`${this.baseUrl}/confirm-reservation`, reservationDto, {}).subscribe({
				next: result => {
					console.debug(`confirmReservation: url ${this.baseUrl}/confirm-reservation result`, result);
					this.reservationsUpdated.next(); // Notify subscribers
					this._userService.updateCurrentUser();
					const mappedReservation = this.mapResponseToReservation(result);
					resolve(mappedReservation);
				},
				error: err => {
					console.warn(`confirmReservation: url ${this.baseUrl}/confirm-reservation`, err);
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
	
	

	deleteReservationById(id: number): Promise<void> {
		return new Promise((resolve, reject) => {
			this._httpClient.delete<void>(`${this.baseUrl}/${id}`).subscribe({
				next: result => {
					console.debug(`deleteReservationById: url ${this.baseUrl}/${id} result`, result);
          this.reservationsUpdated.next();
					this._userService.updateCurrentUser();
					resolve();
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
					console.debug(`updateReservations: url ${this.baseUrl}/update-reservations result`, result);
					this.reservationsUpdated.next();
					this._userService.updateCurrentUser();
					resolve(result);
				},
				error: err => {
					console.warn(`updateReservations: url ${this.baseUrl}/update-reservations`, err);
					reject(err);
				}
			});
		});
	}

	getLegacyReservationsByUserId(userId: number): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<ReservationResponseDTO[]>(`${this.baseUrl}/legacy/user/${userId}`).subscribe({
				next: result => {
					console.debug(`getLegacyReservationsByUserId: url ${this.baseUrl}/legacy/user/${userId} result`, result);
					const mappedReservations = result.map(dto => this.mapResponseToReservation(dto));
					resolve(mappedReservations);
				},
				error: err => {
					console.warn(`getLegacyReservationsByUserId: url ${this.baseUrl}/legacy/user/${userId}`, err);
					if (err.status === 403) {
						reject(new Error('You do not have permission to view these reservations.'));
					} else {
						reject(err);
					}
				}
			});
		});
	}

	getLegacyReservationsByBoatId(boatId: number): Promise<IReservation[]> {
		return new Promise((resolve, reject) => {
			this._httpClient.get<ReservationResponseDTO[]>(`${this.baseUrl}/legacy/boat/${boatId}`).subscribe({
				next: result => {
					console.debug(`getLegacyReservationsByBoatId: url ${this.baseUrl}/legacy/boat/${boatId} result`, result);
					const mappedReservations = result.map(dto => this.mapResponseToReservation(dto));
					resolve(mappedReservations);
				},
				error: err => {
					console.warn(`getLegacyReservationsByBoatId: url ${this.baseUrl}/legacy/boat/${boatId}`, err);
					if (err.status === 403) {
						reject(new Error('You do not have permission to view this boat\'s history.'));
					} else {
						reject(err);
					}
				}
			});
		});
	}

	/**
	 * Formats a date/time as a local datetime string (YYYY-MM-DDTHH:mm:ss) without timezone
	 * This format allows the backend to interpret it as Brazil time, regardless of user's timezone
	 */
	private formatLocalDateTime(year: number, month: number, day: number, hours: number, minutes: number, seconds: number): string {
		const pad = (n: number) => n.toString().padStart(2, '0');
		return `${year}-${pad(month)}-${pad(day)}T${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
	}
}