import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';
import { UserService } from '../../services/user.service';
import { ReservationService } from '../../services/reservation.service';

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
	reservationsByUserId: IReservation[] = [];
	user: IUser | null = null;
	userId: number | null = null;

	currentUser: IUser | null = null;
  
	constructor(
	  private _userService: UserService,
	  private _reservationService: ReservationService,
	) {
	}
  
	ngOnInit(): void {
		this.getAllUsers();

		this._userService.currentUser$.subscribe(user => {
			this.currentUser = user;
		});
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
}