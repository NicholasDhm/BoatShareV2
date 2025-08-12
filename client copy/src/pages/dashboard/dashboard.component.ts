import { Component } from '@angular/core';
import { UiCalendarComponent } from '../../components/ui-calendar/ui-calendar.component';
import { UiCardComponent } from "../../components/ui-card/ui-card.component";
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';
import { IBoat } from '../../models/boat';
import { ICalendarViewModel, IDay } from '../../models/calendar';
import { UserService } from '../../services/user.service';
import { BoatService } from '../../services/boat.service';
import { ReservationService } from '../../services/reservation.service';
import { CalendarService } from '../../services/calendar.service';
import { UiReservationDescriptionComponent } from '../../components/ui-reservation-description/ui-reservation-description.component';
import { ReservationModalComponent } from '../reservation-modal/reservation-modal.component';
import { DashboardCommonService } from '../../services/dashboard-common.service';
import { ValidationService } from '../../services/validation.service';

@Component({
	selector: 'app-dashboard',
	standalone: true,
	imports: [
		UiCalendarComponent,
		UiCardComponent,
		ReservationModalComponent,
		UiCardComponent,
		UiReservationDescriptionComponent
	],
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  currentUser: IUser | null = null;
	firstReservationUser: IUser | null = null;

  boat: IBoat | null = null;
	calendarViewModel: ICalendarViewModel | null = null;

	validationError = '';

	showModal = false;
	userReservationToDisplay: IReservation | null = null;
	firstReservationToDisplay: IReservation | null = null;


	constructor(
		private _userService: UserService,
		private _boatService: BoatService,
		private _reservationService: ReservationService,
		private _calendarService: CalendarService,
		private _dashboardCommonService: DashboardCommonService,
		private _validationService: ValidationService,
	) {
		this.currentUser = this._userService.getCurrentUser();
		this.calendarViewModel = this._calendarService.getCalendarViewModel();
	}

	ngOnInit(): void {
		this._calendarService.loadCalendarViewModel();
		this._userService.updateCurrentUser();

		this._reservationService.reservationsUpdated$.subscribe(() => {
			this._calendarService.loadCalendarViewModel(this.calendarViewModel?.displayDate);
		});

    this._userService.currentUser$.subscribe(user => {
			if (!user) {
				return;
			}
      this.currentUser = user;
			this._boatService.getBoatByBoatId(this.currentUser.boatId).then(boat => {
				if (boat) {
					this.boat = boat;
				}
			});
    });

		this._calendarService.calendarViewModel$.subscribe(calendarViewModel => {
			this.calendarViewModel = calendarViewModel;
		});
	}

	onDaySelected(day: IDay): void {
		if (!this.currentUser) {
			return;
		}
		this._dashboardCommonService.fetchReservationToDisplay(day, this.currentUser).then(reservationsToDisplay => {
			if (!reservationsToDisplay || reservationsToDisplay.length === 0) {
				return;
			}

			if (reservationsToDisplay.length === 1) {
				this.userReservationToDisplay = reservationsToDisplay[0];
				this.firstReservationToDisplay = null;
			} else {
				this.userReservationToDisplay = reservationsToDisplay[1];
				this.firstReservationToDisplay = reservationsToDisplay[0];
			}
			if (this.firstReservationToDisplay) {
				this._userService.getUserById(this.firstReservationToDisplay?.userId).then(user => {
					this.firstReservationUser = user;
					this.showModal = true;
				});
			} else {
				this.firstReservationUser = null;
				this.showModal = true;
			}
		});
	}

	onMonthChanged(month: number): void {
		this._calendarService.updateCalendarMonth(month);
	}

	makeReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult) return;

		const error = this._validationService.validateQuota(this.currentUser, reservationResult.type);
		if (error) {
			this.validationError = error;
			setTimeout(() => {
				this.validationError = '';
			}, 5000);
		} else {
			this.validationError = '';
			this._reservationService.createReservation(reservationResult);
		}
	}

	cancelReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult?.reservationId) {
			return;
		}
		this._reservationService.deleteReservationById(reservationResult.reservationId);
	}

	confirmReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult?.reservationId) {
			return;
		}
		this._reservationService.confirmReservation(reservationResult);
	}


	closeModal(): void {
		this.showModal = false;
	}

}