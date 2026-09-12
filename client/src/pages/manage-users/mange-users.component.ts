import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';
import { IBoat } from '../../models/boat';
import { IChartBar, IChartSlice } from '../../models/chart';
import { UserService } from '../../services/user.service';
import { ReservationService } from '../../services/reservation.service';
import { BoatService } from '../../services/boat.service';
import { UiStatTileComponent } from '../../components/charts/ui-stat-tile/ui-stat-tile.component';
import { UiDonutChartComponent } from '../../components/charts/ui-donut-chart/ui-donut-chart.component';
import { UiBarChartComponent } from '../../components/charts/ui-bar-chart/ui-bar-chart.component';

const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

@Component({
	selector: 'app-manage-users',
	standalone: true,
	imports: [
		CommonModule,
		FormsModule,
		UiStatTileComponent,
		UiDonutChartComponent,
		UiBarChartComponent,
	],
	templateUrl: './manage-users.component.html',
	styleUrls: ['./manage-users.component.scss']
})
export class ManageUsersComponent implements OnInit {
	users: IUser[] = [];
	boats: IBoat[] = [];
	reservationsByUserId: IReservation[] = [];
	user: IUser | null = null;
	/** GET /users devolve UserListDTO, que não traz cotas — o detalhe vem de GET /users/{id}. */
	userDetail: IUser | null = null;
	userId: number | null = null;
	currentUser: IUser | null = null;

	searchTerm = '';

	constructor(
		private _userService: UserService,
		private _reservationService: ReservationService,
		private _boatService: BoatService,
	) {}

	ngOnInit(): void {
		this.getAllUsers();

		this._userService.currentUser$.subscribe(user => {
			this.currentUser = user;
		});

		this._boatService.boats$.subscribe(boats => {
			this.boats = boats;
		});

		this._boatService.getAllBoats();
	}

	// --- Selection -----------------------------------------------------------

	onSelectUserId(userId: number | null): void {
		if (!userId) {
			this.user = null;
			this.userDetail = null;
			return;
		}

		this.userId = userId;
		this.user = this.users?.find(x => x.userId === userId) || null;
		this.userDetail = null;
		this.getReservationsByUserId(userId);

		this._userService.getUserById(userId)
			.then(detail => {
				if (this.userId === detail.userId) {
					this.userDetail = detail;
				}
			})
			.catch(error => console.warn('Error loading user detail:', error));
	}

	get filteredUsers(): IUser[] {
		const term = this.searchTerm.trim().toLowerCase();
		const users = term
			? this.users.filter(user => user.name.toLowerCase().includes(term) || user.email.toLowerCase().includes(term))
			: this.users;

		return [...users].sort((a, b) => a.name.localeCompare(b.name, 'pt-BR'));
	}

	// --- Insights ------------------------------------------------------------

	get adminCount(): number {
		return this.users.filter(user => user.role === 'Admin').length;
	}

	/** APIs anteriores a esta versão não devolvem cotas na listagem — sem isso os
	    agregados seriam zeros silenciosos, então eles somem da tela. */
	get hasQuotaData(): boolean {
		return this.users.some(user => Number.isFinite(user.standardQuota));
	}

	get totalQuotasInPlay(): number {
		return this.users.reduce(
			(total, user) => total + (user.standardQuota ?? 0) + (user.substitutionQuota ?? 0) + (user.contingencyQuota ?? 0),
			0
		);
	}

	/** Saldo de cotas por cotista — uma série, do maior para o menor. */
	get quotasPerUserBars(): IChartBar[] {
		return this.users
			.map(user => ({
				label: user.name.split(' ')[0],
				fullLabel: user.name,
				value: (user.standardQuota ?? 0) + (user.substitutionQuota ?? 0) + (user.contingencyQuota ?? 0)
			}))
			.sort((a, b) => b.value - a.value)
			.slice(0, 12);
	}

	get quotaSlices(): IChartSlice[] {
		if (!this.userDetail) {
			return [];
		}
		return [
			{ label: 'Padrão', value: this.userDetail.standardQuota ?? 0, color: 'var(--series-standard)' },
			{ label: 'Suplência', value: this.userDetail.substitutionQuota ?? 0, color: 'var(--series-substitution)' },
			{ label: 'Contingência', value: this.userDetail.contingencyQuota ?? 0, color: 'var(--series-contingency)' },
		];
	}

	get userTotalQuotas(): number {
		return this.quotaSlices.reduce((total, slice) => total + slice.value, 0);
	}

	get isQuotaDetailLoading(): boolean {
		return this.user !== null && this.userDetail === null;
	}

	// --- Actions -------------------------------------------------------------

	changeUserRole(): void {
		if (!this.user) {
			return;
		}
		const userWithNewRole: IUser = { ...this.user, role: this.user.role === 'Admin' ? 'Member' : 'Admin' };
		this._userService.updateUser(userWithNewRole).then(() => {
			this.getAllUsers();
		});
	}

	// --- Helpers -------------------------------------------------------------

	getUserReservations(): IReservation[] {
		if (!this.user) return [];
		return this.reservationsByUserId
			.filter(reservation => reservation.userId === this.user?.userId)
			.sort((a, b) => new Date(a.year, a.month - 1, a.day).getTime() - new Date(b.year, b.month - 1, b.day).getTime());
	}

	formatReservationDate(day: number, month: number, year: number): string {
		return `${day} de ${MONTHS_LONG[month - 1]} de ${year}`;
	}

	getRoleLabel(role: string): string {
		return role === 'Admin' ? 'Administrador' : 'Membro';
	}

	getBoatName(boatId: number | undefined): string {
		if (!boatId) return '—';
		const boat = this.boats.find(b => b.boatId === boatId);
		return boat ? boat.name : `Barco #${boatId}`;
	}

	initials(name: string): string {
		return name
			.split(' ')
			.filter(part => part.length > 0)
			.slice(0, 2)
			.map(part => part[0].toUpperCase())
			.join('');
	}

	typeLabel(type: string): string {
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

	private getAllUsers(): void {
		this._userService.getAllUsers().then(users => {
			this.users = users;
			if (users?.length > 0) {
				this.onSelectUserId(this.userId ?? users[0].userId);
			}
		});
	}

	private getReservationsByUserId(userId: number): void {
		this._reservationService.getReservationsByUserId(userId).then(reservations => {
			reservations.forEach(reservation => {
				if (!this.reservationsByUserId.some(r => r.reservationId === reservation.reservationId)) {
					this.reservationsByUserId.push(reservation);
				}
			});
		});
	}
}
