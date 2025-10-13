import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';
import { IBoat } from '../../models/boat';
import { UserService } from '../../services/user.service';
import { ReservationService } from '../../services/reservation.service';
import { BoatService } from '../../services/boat.service';

@Component({
	selector: 'app-manage-users',
	standalone: true,
	imports: [
	  UiCardComponent,
	  CommonModule,
	  FormsModule,
	],
	templateUrl: './manage-users.component.html',
	styleUrls: ['./manage-users.component.scss']
})
export class ManageUsersComponent implements OnInit {
	users: IUser[] = [];
	boats: IBoat[] = [];
	reservationsByUserId: IReservation[] = [];
	user: IUser | null = null;
	userId: number | null = null;

	currentUser: IUser | null = null;

	constructor(
	  private _userService: UserService,
	  private _reservationService: ReservationService,
	  private _boatService: BoatService,
	) {
	}

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

	private getAllUsers(): void {
		this._userService.getAllUsers().then(users => {
			this.users = users;
			if (users?.length > 0) {
				const firstUser = users[0];
				this.onSelectUserId(firstUser.userId);
			}
		});
	}
  
	onSelectUserId(userId: number | null): void {
	  if (userId) {
			this.userId = userId;
			this.user = this.users?.find(x => x.userId === userId) || null;
			this.getReservationsByUserId(userId);
	  } else {
			this.user = null;
	  }
	}
  
	private getReservationsByUserId(userId: number) {
	  this._reservationService.getReservationsByUserId(userId).then(reservationsByUserId => {
		reservationsByUserId.forEach(reservation => {
		  if (!this.reservationsByUserId.some(r => r.reservationId === reservation.reservationId)) {
			this.reservationsByUserId.push(reservation);
		  }
		});
	  });
	}

	changeUserRole(): void {
		if (this.user) {
			const userWithNewRole: IUser = { ...this.user, role: this.user.role === 'Admin' ? 'Member' : 'Admin' };
			this._userService.updateUser(userWithNewRole).then(() => {
				this.getAllUsers();
			});
		}
	}

	getUserReservations(): IReservation[] {
		if (!this.user) return [];
		return this.reservationsByUserId.filter(r => r.userId === this.user?.userId);
	}

	formatReservationDate(day: number, month: number, year: number): string {
		const monthNames = [
			'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
			'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
		];
		return `${day} de ${monthNames[month - 1]} de ${year}`;
	}

	getRoleLabel(role: string): string {
		return role === 'Admin' ? 'Administrador' : 'Membro';
	}

	getBoatName(boatId: number | undefined): string {
		if (!boatId) return 'N/A';
		const boat = this.boats.find(b => b.boatId === boatId);
		return boat ? boat.name : `Barco #${boatId}`;
	}
}