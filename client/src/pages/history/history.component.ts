import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IReservation } from '../../models/reservation';
import { IUser } from '../../models/user';
import { ReservationService } from '../../services/reservation.service';
import { UserService } from '../../services/user.service';
import { UiLoadingSpinnerComponent } from '../../components/ui-loading-spinner/ui-loading-spinner.component';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';

@Component({
	selector: 'app-history',
	standalone: true,
	imports: [
		CommonModule,
		UiLoadingSpinnerComponent,
		UiCardComponent
	],
	templateUrl: './history.component.html',
	styleUrls: ['./history.component.scss']
})
export class HistoryComponent implements OnInit {
	currentUser: IUser | null = null;
	viewMode: 'user' | 'boat' = 'user';

	userLegacyReservations: IReservation[] = [];
	boatLegacyReservations: IReservation[] = [];

	isLoadingUserHistory = false;
	isLoadingBoatHistory = false;

	errorMessage = '';

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

	toggleView(mode: 'user' | 'boat'): void {
		this.viewMode = mode;
		if (mode === 'boat' && this.boatLegacyReservations.length === 0) {
			this.loadBoatHistory();
		}
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

	getReservationTypeLabel(type: string): string {
		switch (type) {
			case 'Standard': return 'Padrão';
			case 'Substitution': return 'Suplência';
			case 'Contingency': return 'Contingência';
			default: return type;
		}
	}

	getStatusLabel(status: string): string {
		switch (status) {
			case 'Confirmed': return 'Confirmada';
			case 'Pending': return 'Pendente';
			case 'Unconfirmed': return 'Não Confirmada';
			case 'Cancelled': return 'Cancelada';
			case 'Legacy': return 'Arquivada';
			default: return status;
		}
	}

	formatDate(year: number, month: number, day: number): string {
		const date = new Date(year, month - 1, day);
		return date.toLocaleDateString('pt-BR', {
			weekday: 'long',
			year: 'numeric',
			month: 'long',
			day: 'numeric'
		});
	}

	getDisplayedReservations(): IReservation[] {
		return this.viewMode === 'user' ? this.userLegacyReservations : this.boatLegacyReservations;
	}

	isLoading(): boolean {
		return this.viewMode === 'user' ? this.isLoadingUserHistory : this.isLoadingBoatHistory;
	}
}
