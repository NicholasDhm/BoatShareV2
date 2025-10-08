import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from "../../components/ui-card/ui-card.component";
import { IUser } from '../../models/user';
import { UserService } from '../../services/user.service';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';
import { IReservation } from '../../models/reservation';
import { ReservationService } from '../../services/reservation.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [UiCardComponent, ReactiveFormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: IUser | null = null;
  boat: IBoat | null = null;
  reservations: IReservation[] = [];

  isEditing = false;
	newPasswordForm: FormGroup;

  constructor(
    private _userService: UserService,
    private _boatService: BoatService,
    private _authService: AuthService,
    private _reservationService: ReservationService,
		private fb: FormBuilder,
	) {
		this.newPasswordForm = this.fb.group({
			password: ['', [Validators.required, Validators.minLength(6)]],
		});
  }

	ngOnInit(): void {
    this._userService.currentUser$.subscribe(user => {
      if (user) {
        this.user = user;
        this._boatService.getBoatByBoatId(user.boatId).then(boat => {
          this.boat = boat;
        }
        );
        this._reservationService.getReservationsByUserId(user.userId).then(reservations => {
          this.reservations = reservations;
        });
      }
    });
  }

  changeEditState(): void {
    this.isEditing = !this.isEditing;
    this.newPasswordForm.reset();
  }

  confirmChanges(): void {
    if (!this.user) {
      return;
    }
    this._authService.updateUserPassword(this.user.userId, this.newPasswordForm.value.password).then(() => {
      this.changeEditState();
    });
  }

}