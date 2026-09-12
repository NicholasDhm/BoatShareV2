import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiCalendarComponent } from '../../components/ui-calendar/ui-calendar.component';
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';
import { IBoat } from '../../models/boat';
import { ICalendarViewModel, IDay } from '../../models/calendar';
import { IChartBar, IChartSlice } from '../../models/chart';
import { UserService } from '../../services/user.service';
import { BoatService } from '../../services/boat.service';
import { ReservationService } from '../../services/reservation.service';
import { CalendarService } from '../../services/calendar.service';
import { UiReservationDescriptionComponent } from '../../components/ui-reservation-description/ui-reservation-description.component';
import { UiLoadingSpinnerComponent } from '../../components/ui-loading-spinner/ui-loading-spinner.component';
import { UiStatTileComponent } from '../../components/charts/ui-stat-tile/ui-stat-tile.component';
import { UiDonutChartComponent } from '../../components/charts/ui-donut-chart/ui-donut-chart.component';
import { UiBarChartComponent } from '../../components/charts/ui-bar-chart/ui-bar-chart.component';
import { ReservationModalComponent } from '../reservation-modal/reservation-modal.component';
import { DashboardCommonService } from '../../services/dashboard-common.service';
import { ValidationService } from '../../services/validation.service';

const MONTHS_SHORT = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

@Component({
	selector: 'app-dashboard',
	standalone: true,
	imports: [
		CommonModule,
		UiCalendarComponent,
		ReservationModalComponent,
		UiReservationDescriptionComponent,
		UiLoadingSpinnerComponent,
		UiStatTileComponent,
		UiDonutChartComponent,
		UiBarChartComponent,
	],
	templateUrl: './dashboard.component.html',
	styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
	currentUser: IUser | null = null;
	firstReservationUserName: string | null = null;

	boat: IBoat | null = null;
	calendarViewModel: ICalendarViewModel | null = null;
	userReservations: IReservation[] = [];

	validationError = '';

	showModal = false;
	userReservationToDisplay: IReservation | null = null;
	firstReservationToDisplay: IReservation | null = null;

	statusSlices: IChartSlice[] = [];
	monthlyBars: IChartBar[] = [];
	boatMonthlyBars: IChartBar[] = [];
	boatDaysThisMonth = 0;
	nextReservationLabel = '—';

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
		this.refreshInsights();
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
			this.refreshInsights();
			this.loadUserReservations();
			this._boatService.getBoatByBoatId(this.currentUser.boatId).then(boat => {
				if (boat) {
					this.boat = boat;
				}
			});
		});

		this._calendarService.calendarViewModel$.subscribe(calendarViewModel => {
			this.calendarViewModel = calendarViewModel;
			this.refreshBoatInsights();
		});
	}

	// --- Presentation helpers ------------------------------------------------

	get greeting(): string {
		const hour = new Date().getHours();
		if (hour < 12) return 'Bom dia';
		if (hour < 18) return 'Boa tarde';
		return 'Boa noite';
	}

	get firstName(): string {
		return this.currentUser?.name?.split(' ')[0] ?? '';
	}

	get currentYear(): number {
		return new Date().getFullYear();
	}

	get todayLabel(): string {
		const today = new Date();
		return `${today.getDate()} de ${MONTHS_LONG[today.getMonth()]} de ${today.getFullYear()}`;
	}

	get boatLabel(): string {
		return this.boat?.name ?? this.currentUser?.boatName ?? '—';
	}

	get activeReservations(): number {
		return this.userReservations.filter(r => r.status === 'Confirmed' || r.status === 'Pending').length;
	}

	get confirmedCount(): number {
		return this.userReservations.filter(r => r.status === 'Confirmed').length;
	}

	get pendingCount(): number {
		return this.userReservations.filter(r => r.status === 'Pending').length;
	}

	get awaitingConfirmationCount(): number {
		return this.userReservations.filter(r => r.status === 'Unconfirmed').length;
	}

	get availableQuotas(): number {
		if (!this.currentUser) return 0;
		return this.currentUser.standardQuota + this.currentUser.substitutionQuota + this.currentUser.contingencyQuota;
	}

	// --- Calendar / reservation flow ----------------------------------------

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
				this.firstReservationUserName = null;
			} else {
				this.userReservationToDisplay = reservationsToDisplay[1];
				this.firstReservationToDisplay = reservationsToDisplay[0];
				this.firstReservationUserName = this.firstReservationToDisplay?.userName || null;
			}
			this.showModal = true;
		});
	}

	onMonthChanged(month: number): void {
		this._calendarService.updateCalendarMonth(month);
	}

	makeReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult) return;

		const error = this._validationService.validateQuota(this.currentUser, reservationResult.type);
		if (error) {
			this.showError(error);
			return;
		}

		this.validationError = '';
		this._reservationService.createReservation(reservationResult).catch(error => {
			console.error('Error creating reservation:', error);
			if (error.error?.message?.includes('quota')) {
				this.showError('Erro: ' + error.error.message);
			} else {
				this.showError('Erro ao criar reserva. Tente novamente.');
			}
		});
	}

	cancelReservation(reservationResult: IReservation | null): void {
		if (!this.currentUser || !reservationResult?.reservationId) {
			return;
		}
		this._reservationService.deleteReservationById(reservationResult.reservationId).catch(error => {
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

	// --- Internals -----------------------------------------------------------

	private showError(message: string): void {
		this.validationError = message;
		setTimeout(() => {
			this.validationError = '';
		}, 5000);
	}

	private loadUserReservations(): void {
		if (!this.currentUser) return;

		this._reservationService.getReservationsByUserId(this.currentUser.userId).then(reservations => {
			this.userReservations = reservations || [];
			this.refreshInsights();
		}).catch(error => {
			console.warn('Error loading user reservations:', error);
			this.userReservations = [];
			this.refreshInsights();
		});
	}

	private refreshInsights(): void {
		this.statusSlices = [
			{ label: 'Confirmadas', value: this.confirmedCount, color: 'var(--status-good)' },
			{ label: 'Pendentes', value: this.pendingCount, color: 'var(--status-warning)' },
			{ label: 'A confirmar', value: this.awaitingConfirmationCount, color: 'var(--status-critical)' },
		];

		this.monthlyBars = this.countByMonth(this.userReservations);
		this.nextReservationLabel = this.findNextReservation();
	}

	private refreshBoatInsights(): void {
		const occupied = this.calendarViewModel?.occupiedDates ?? [];
		this.boatMonthlyBars = this.countByMonth(occupied);

		const today = new Date();
		this.boatDaysThisMonth = occupied.filter(
			reservation => reservation.year === today.getFullYear() && reservation.month === today.getMonth() + 1
		).length;
	}

	private countByMonth(reservations: IReservation[]): IChartBar[] {
		const year = new Date().getFullYear();
		const perMonth = new Array(12).fill(0);

		reservations
			.filter(reservation => reservation.year === year)
			.forEach(reservation => {
				perMonth[reservation.month - 1] += 1;
			});

		return perMonth.map((value, index) => ({
			label: MONTHS_SHORT[index],
			fullLabel: `${MONTHS_LONG[index]} de ${year}`,
			value
		}));
	}

	private findNextReservation(): string {
		const today = new Date();
		today.setHours(0, 0, 0, 0);

		const upcoming = this.userReservations
			.map(reservation => ({ reservation, date: new Date(reservation.year, reservation.month - 1, reservation.day) }))
			.filter(entry => entry.date >= today)
			.sort((a, b) => a.date.getTime() - b.date.getTime())[0];

		if (!upcoming) {
			return '—';
		}
		return `${upcoming.date.getDate()} de ${MONTHS_LONG[upcoming.date.getMonth()]}`;
	}
}
