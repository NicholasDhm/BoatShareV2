import { Component, OnInit, Signal } from '@angular/core';
import { UiCardComponent } from '../components/ui-card/ui-card.component';
import { CommonModule, JsonPipe } from '@angular/common';
import { IUserTestingViewModel, UserTestPageService } from './test-page-services/user-test-page.service';
import { BoatTestPageService, IBoatTestingViewModel } from './test-page-services/boat-test-page.service';
import { IReservationTestingViewModel, ReservationTestPageService } from './test-page-services/reservation-test-page.service';
import { UiCalendarComponent } from '../components/ui-calendar/ui-calendar.component';
import { DateManagerService } from '../services/date-manager.service';
import { DateTime } from 'luxon';
import { Subscription } from 'rxjs';

@Component({
	selector: 'app-testing-page',
	standalone: true,
	imports: [UiCardComponent, UiCalendarComponent, JsonPipe, CommonModule],
	templateUrl: './testing-page.component.html',
	styleUrls: ['./testing-page.component.scss']
})
export class TestingPageComponent implements OnInit {
	boatResult: any;
	userResult: any;
	reservationResult: any;
	result = '';
	updationResult = '';

	currentDateTime: DateTime = DateTime.now();
	currentHour: number = DateTime.now().hour;

	userViewModel: Signal<IUserTestingViewModel>;
	boatViewModel: Signal<IBoatTestingViewModel>;
	reservationViewModel: Signal<IReservationTestingViewModel>;

	private _subscription: Subscription = new Subscription();

	constructor(
		private _userTestPageService: UserTestPageService,
		private _boatTestPageService: BoatTestPageService,
		private _reservationTestPageService: ReservationTestPageService,
		private _dateManagerService: DateManagerService,
	) {
		this.userViewModel = this._userTestPageService.viewModel;
		this.boatViewModel = this._boatTestPageService.viewModel;
		this.reservationViewModel = this._reservationTestPageService.viewModel;
	}

	ngOnInit(): void {
		this._subscription.add(this._dateManagerService.currentDate$.subscribe(datetime => {
			this.currentDateTime = datetime;
		}));
		this._subscription.add(this._dateManagerService.currentTime$.subscribe(hour => {
			this.currentHour = hour;
		}));
	}

	deletePastReservations() {
		this._reservationTestPageService.deletePastReservations().then(() => {
			this.result = "Success";
		});
	}


	updateReservations() {
		this._reservationTestPageService.updateReservations().then(() => {
			this.updationResult = "Success";
		});
	}


  advanceDate() {
    this._dateManagerService.incrementDate(1); // Advance by one day
  }

  regressDate() {
    this._dateManagerService.decrementDate(1); // Go back by one day
  }

  setContingencyTime(event: any) {
    this._dateManagerService.setContingencyTime(event.target.value);
  }

  resetTime() {
    this._dateManagerService.resetTime(); // Reset to current time
  }

	// User Methods
	getAllUsers(): void {
		this._userTestPageService.getAllUsers().then(users => {
			this.userResult = users;
		});
	}

	searchUserByPartialName(): void {
		this._userTestPageService.updateViewModel(this._userTestPageService.viewModel().searchQuery, 'searchQuery');
		this._userTestPageService.searchUsers();
	}

	getUserById(): void {
		this._userTestPageService.getUserById().then(user => {
			this.userResult = user;
		});
	}

	addQuota(): void {
		this._userTestPageService.addQuota().then(user => {
			this.userResult = user;
		});
	}

	// postRegularUser(): void {
	// 	this._userTestPageService.postRegularUser().then(user => {
	// 		this.userResult = user;
	// 	});
	// }

	// postUserAsAdmin(): void {
	// 	this._userTestPageService.postUserAsAdmin().then(user => {
	// 		this.userResult = user;
	// 	});
	// }

	deleteUserById(): void {
		this._userTestPageService.deleteUserById().then(user => {
			this.userResult = user;
		});
	}

	updateUserViewModel(event: any, type: keyof IUserTestingViewModel): void {
		this._userTestPageService.updateViewModel(event.target.value, type);
	}

	// Boat Methods

	updateBoatViewModel(event: any, type: keyof IBoatTestingViewModel): void {
		this._boatTestPageService.updateViewModel(event.target.value, type);
	}

	getAllBoats(): void {
		this._boatTestPageService.getAllBoats().then(boats => {
			this.boatResult = boats;
		});
	}

	getBoatById(): void {
		this._boatTestPageService.getBoatById().then(boat => {
			this.boatResult = boat;
		});
	}

	postBoat(): void {
		this._boatTestPageService.postBoat().then(boat => {
			this.boatResult = boat;
		});
	}

	deleteBoatById(): void {
		this._boatTestPageService.deleteBoatById();
	}

	// Reservation Methods

	updateReservationViewModel(event: any, type: keyof IReservationTestingViewModel): void {
		this._reservationTestPageService.updateViewModel(event.target.value, type);
	}

	getAllReservations(): void {
		this._reservationTestPageService.getAllReservations().then(reservations => {
			this.reservationResult = reservations;
		});
	}

	// getReservationByBoatId(): void {
	// 	this._reservationTestPageService.getReservationByBoatId().then(reservation => {
	// 		this.reservationResult = reservation;
	// 	});
	// }

	postReservation(): void {
		this._reservationTestPageService.postReservation().then(reservation => {
			this.reservationResult = reservation;
		});
	}

	getOccupiedDatesForYear(): void {
		this._reservationTestPageService.getOccupiedDatesForYear().then(reservations => {
			this.reservationResult = reservations;
		});
	}

	deleteReservationById(): void {
		this._reservationTestPageService.deleteReservationById().then(reservation => {
			this.reservationResult = reservation;
		});
	}

}