import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IReservation, ReservationType } from '../../models/reservation';
import { IUser } from '../../models/user';
import { IChartBar, IChartSlice } from '../../models/chart';
import { ReservationService } from '../../services/reservation.service';
import { UserService } from '../../services/user.service';
import { UiLoadingSpinnerComponent } from '../../components/ui-loading-spinner/ui-loading-spinner.component';
import { UiStatTileComponent } from '../../components/charts/ui-stat-tile/ui-stat-tile.component';
import { UiBarChartComponent } from '../../components/charts/ui-bar-chart/ui-bar-chart.component';
import { UiDonutChartComponent } from '../../components/charts/ui-donut-chart/ui-donut-chart.component';

const MONTHS_SHORT = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

type TypeFilter = 'All' | ReservationType;

@Component({
	selector: 'app-history',
	standalone: true,
	imports: [
		CommonModule,
		UiLoadingSpinnerComponent,
		UiStatTileComponent,
		UiBarChartComponent,
		UiDonutChartComponent,
	],
	templateUrl: './history.component.html',
	styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
	currentUser: IUser | null = null;
	viewMode: 'user' | 'boat' = 'user';
	typeFilter: TypeFilter = 'All';

	userLegacyReservations: IReservation[] = [];
	boatLegacyReservations: IReservation[] = [];

	isLoadingUserHistory = false;
	isLoadingBoatHistory = false;

	errorMessage = '';

	readonly typeFilters: { key: TypeFilter; label: string }[] = [
		{ key: 'All', label: 'Todos os tipos' },
		{ key: 'Standard', label: 'Padrão' },
		{ key: 'Substitution', label: 'Suplência' },
		{ key: 'Contingency', label: 'Contingência' },
	];

	constructor(
		private _reservationService: ReservationService,
		private _userService: UserService
	) {}

	ngOnInit(): void {
		this.currentUser = this._userService.getCurrentUser();
		if (!this.currentUser) {
			this.errorMessage = 'Por favor, faça login para ver o histórico';
			return;
		}
		this.loadUserHistory();
	}

	// --- Data ----------------------------------------------------------------

	toggleView(mode: 'user' | 'boat'): void {
		this.viewMode = mode;
		if (mode === 'boat' && this.boatLegacyReservations.length === 0) {
			this.loadBoatHistory();
		}
	}

	setTypeFilter(filter: TypeFilter): void {
		this.typeFilter = filter;
	}

	loadUserHistory(): void {
		if (!this.currentUser) return;

		this.isLoadingUserHistory = true;
		this.errorMessage = '';

		this._reservationService.getLegacyReservationsByUserId(this.currentUser.userId)
			.then(reservations => {
				this.userLegacyReservations = reservations;
			})
			.catch(error => {
				console.error('Error loading user history:', error);
				this.errorMessage = error.message || 'Falha ao carregar histórico';
			})
			.finally(() => {
				this.isLoadingUserHistory = false;
			});
	}

	loadBoatHistory(): void {
		if (!this.currentUser) return;

		this.isLoadingBoatHistory = true;
		this.errorMessage = '';

		this._reservationService.getLegacyReservationsByBoatId(this.currentUser.boatId)
			.then(reservations => {
				this.boatLegacyReservations = reservations;
			})
			.catch(error => {
				console.error('Error loading boat history:', error);
				this.errorMessage = error.message || 'Falha ao carregar histórico do barco';
			})
			.finally(() => {
				this.isLoadingBoatHistory = false;
			});
	}

	isLoading(): boolean {
		return this.viewMode === 'user' ? this.isLoadingUserHistory : this.isLoadingBoatHistory;
	}

	/** Everything in the current view, before the type filter. */
	get scopedReservations(): IReservation[] {
		return this.viewMode === 'user' ? this.userLegacyReservations : this.boatLegacyReservations;
	}

	/** What the table, the charts and the stats all read from. */
	getDisplayedReservations(): IReservation[] {
		const reservations = this.typeFilter === 'All'
			? this.scopedReservations
			: this.scopedReservations.filter(reservation => reservation.type === this.typeFilter);

		return [...reservations].sort((a, b) =>
			new Date(b.year, b.month - 1, b.day).getTime() - new Date(a.year, a.month - 1, a.day).getTime()
		);
	}

	// --- Derived insights ----------------------------------------------------

	get totalCount(): number {
		return this.getDisplayedReservations().length;
	}

	get monthlyBars(): IChartBar[] {
		const perMonth = new Array(12).fill(0);
		this.getDisplayedReservations().forEach(reservation => {
			perMonth[reservation.month - 1] += 1;
		});

		return perMonth.map((value, index) => ({
			label: MONTHS_SHORT[index],
			fullLabel: `${MONTHS_LONG[index]} (todos os anos)`,
			value
		}));
	}

	get typeSlices(): IChartSlice[] {
		const reservations = this.typeFilter === 'All' ? this.scopedReservations : this.getDisplayedReservations();
		const count = (type: ReservationType) => reservations.filter(reservation => reservation.type === type).length;

		return [
			{ label: 'Padrão', value: count('Standard'), color: 'var(--series-standard)' },
			{ label: 'Suplência', value: count('Substitution'), color: 'var(--series-substitution)' },
			{ label: 'Contingência', value: count('Contingency'), color: 'var(--series-contingency)' },
		];
	}

	get busiestMonth(): string {
		const bars = this.monthlyBars;
		const peak = Math.max(...bars.map(bar => bar.value));
		if (peak === 0) {
			return '—';
		}
		return MONTHS_LONG[bars.findIndex(bar => bar.value === peak)];
	}

	get topType(): string {
		const slices = this.typeSlices;
		const peak = Math.max(...slices.map(slice => slice.value));
		if (peak === 0) {
			return '—';
		}
		return slices.find(slice => slice.value === peak)!.label;
	}

	get distinctMembers(): number {
		const names = new Set(this.getDisplayedReservations().map(reservation => reservation.userName).filter(Boolean));
		return names.size;
	}

	// --- Labels --------------------------------------------------------------

	getReservationTypeLabel(type: string): string {
		switch (type) {
			case 'Standard': return 'Padrão';
			case 'Substitution': return 'Suplência';
			case 'Contingency': return 'Contingência';
			default: return type;
		}
	}

	typeKey(type: string): string {
		return type.toLowerCase();
	}

	getStatusLabel(status: string): string {
		switch (status) {
			case 'Confirmed': return 'Confirmada';
			case 'Pending': return 'Pendente';
			case 'Unconfirmed': return 'Não confirmada';
			case 'Cancelled': return 'Cancelada';
			case 'Legacy': return 'Arquivada';
			default: return status;
		}
	}

	getStatusTone(status: string | undefined): string {
		switch (status) {
			case 'Confirmed': return 'good';
			case 'Pending': return 'warning';
			case 'Unconfirmed':
			case 'Cancelled': return 'critical';
			default: return 'neutral';
		}
	}

	getStatusIcon(status: string | undefined): string {
		switch (status) {
			case 'Confirmed': return 'bi-check-circle';
			case 'Pending': return 'bi-hourglass-split';
			case 'Unconfirmed':
			case 'Cancelled': return 'bi-x-circle';
			default: return 'bi-archive';
		}
	}

	formatDate(year: number, month: number, day: number): string {
		return `${day} de ${MONTHS_LONG[month - 1]} de ${year}`;
	}

	formatWeekday(year: number, month: number, day: number): string {
		return new Date(year, month - 1, day).toLocaleDateString('pt-BR', { weekday: 'short' });
	}

	formatCreatedDate(isoDate: string | undefined): string {
		if (!isoDate || isoDate.trim() === '') {
			return '—';
		}

		const date = new Date(isoDate);
		if (isNaN(date.getTime())) {
			return '—';
		}

		return date.toLocaleString('pt-BR', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}
}
