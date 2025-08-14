import { Injectable, Signal, signal } from '@angular/core';
import { ReservationService } from '../../services/reservation.service';
import { IReservation } from '../../models/reservation';

export interface IReservationTestingViewModel {
	getReservationId: string;

	postUserId: string;
	postBoatId: string;
	postYear: number;
	postMonth: number;
	postDay: number;

	deleteReservationId: string;

	year: number;
}

@Injectable({
	providedIn: 'root'
})
export class ReservationTestPageService {

	_viewModel = signal<IReservationTestingViewModel>({
		getReservationId: "1",

		postUserId: "1",
		postBoatId: "1",
		postYear: 2024,
		postMonth: 10,
		postDay: 26,

		deleteReservationId: "1",

		year: 2024,
	});

	constructor(
		private _reservationService: ReservationService,
	) {}

	get viewModel(): Signal<IReservationTestingViewModel> {
		return this._viewModel;
	}

	updateViewModel(value: string | number, type: keyof IReservationTestingViewModel): void {
		this._viewModel.set(Object.assign({}, this._viewModel(), { [type]: value }));
	}

	getAllReservations(): Promise<IReservation[]> {
		return this._reservationService.getAllReservations().then(reservations => {
			return reservations;
		})
	}

	// getReservationByBoatId(): Promise<IReservation> {
	// 	return this._reservationService.getReservationByBoatId(this._viewModel().getReservationId).then(reservation => {
	// 		return reservation;
	// 	})
	// }

	postReservation(): Promise<string> {
		const reservationModel: IReservation = {
			type: 'Standard',
			userId: this._viewModel().postUserId,
			boatId: this._viewModel().postBoatId,
			year: this._viewModel().postYear,
			month: this._viewModel().postMonth,
			day: this._viewModel().postDay,
		};

		return this._reservationService.createReservation(reservationModel).then(reservation => {
			return reservation;
		})
	}

	getOccupiedDatesForYear(): Promise<Date[]> {
		return this._reservationService.getOccupiedDatesForYear(this._viewModel().year).then(dates => {
			return dates;
		})
	}

	deleteReservationById(): Promise<void> {
		return this._reservationService.deleteReservationById(this._viewModel().deleteReservationId);
	}

	deletePastReservations(): Promise<void> {
		return this._reservationService.deletePastReservations().then(() => {
			return;
		})
	}

	updateReservations(): Promise<void> {
		return this._reservationService.updateReservations().then(() => {
			return;
		})
	}

}