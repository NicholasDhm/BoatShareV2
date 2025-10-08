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
import { UiLoadingSpinnerComponent } from '../../components/ui-loading-spinner/ui-loading-spinner.component';
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
		UiReservationDescriptionComponent,
		UiLoadingSpinnerComponent
	],
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  currentUser: IUser | null = null;
	firstReservationUser: IUser | null = null;

  boat: IBoat | null = null;
	calendarViewModel: ICalendarViewModel | null = null;
	userReservations: IReservation[] = [];

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
		// Update reservation statuses first, then load calendar
		this._calendarService.updateReservationStatuses();
		this._userService.updateCurrentUser();

		this._reservationService.reservationsUpdated$.subscribe(() => {
			this._calendarService.loadCalendarViewModel(this.calendarViewModel?.displayDate);
			this.loadUserReservations();
		});

    this._userService.currentUser$.subscribe(user => {
			if (!user) {
				return;
			}
      this.currentUser = user;
			this.loadUserReservations();
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
			this._reservationService.createReservation(reservationResult).then(() => {
				// Reservation created successfully
				console.log('Reservation created successfully');
			}).catch(error => {
				// Handle server-side quota errors
				console.error('Error creating reservation:', error);
				if (error.error && error.error.message && error.error.message.includes('quota')) {
					this.validationError = 'Erro: ' + error.error.message;
				} else {
					this.validationError = 'Erro ao criar reserva. Tente novamente.';
				}
				setTimeout(() => {
					this.validationError = '';
				}, 5000);
			});
		}
	}

	cancelReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult?.reservationId) {
			return;
		}
		this._reservationService.deleteReservationById(reservationResult.reservationId).then(() => {
			// Reservation deleted successfully
			console.log('Reservation cancelled successfully');
		}).catch(error => {
			console.error('Error cancelling reservation:', error);
		});
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

	getTotalQuotas(): number {
		if (!this.currentUser) return 0;
		return this.currentUser.standardQuota + this.currentUser.substitutionQuota + this.currentUser.contingencyQuota;
	}

	getActiveQuotas(): number {
		if (!this.currentUser) return 0;
		// Cotas Ativas = quotas em uso (reservas ativas)
		return this.userReservations.filter(reservation => 
			reservation.status === 'Confirmed' || reservation.status === 'Pending'
		).length;
	}

	getInactiveQuotas(): number {
		if (!this.currentUser) return 0;
		// Cotas Inativas = quotas disponíveis (total - em uso)
		const totalQuotas = this.currentUser.standardQuota + this.currentUser.substitutionQuota + this.currentUser.contingencyQuota;
		const activeQuotas = this.getActiveQuotas();
		return totalQuotas - activeQuotas;
	}

	private loadUserReservations(): void {
		if (!this.currentUser) return;
		
		this._reservationService.getReservationsByUserId(this.currentUser.userId).then(reservations => {
			this.userReservations = reservations || [];
		}).catch(error => {
			console.warn('Error loading user reservations:', error);
			this.userReservations = [];
		});
	}

}