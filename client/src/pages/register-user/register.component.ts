import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { IBoat } from '../../models/boat';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BoatService } from '../../services/boat.service';
import { UserDTO } from '../../models/user';
import { Subscription } from 'rxjs';
import { AuthService } from '../../auth/auth.service';

@Component({
	selector: 'app-register-user',
	standalone: true,
	imports: [UiCardComponent, ReactiveFormsModule],
	templateUrl: './register-user.component.html',
	styleUrls: ['./register-user.component.scss']
})
export class RegisterUserComponent implements OnInit{
  private _subscriptions: Subscription = new Subscription();
	registerUserForm: FormGroup;
	boats: IBoat[] = [];

	constructor(
		private _boatService: BoatService,
		private _authService: AuthService,
		private fb: FormBuilder,
	) {
		this._boatService.fetchBoats();

		this.registerUserForm = this.fb.group({
			name: ['', Validators.required],
			email: ['', [Validators.required, Validators.email]],
			boatId: ['', Validators.required],
			password: ['', Validators.required],
		});
	}

	ngOnInit(): void {
    this._subscriptions.add(this._boatService.boats$.subscribe(boats => {
      this.boats = boats; // Update local boats array
    }));
	}

	registerNewUser() {
		if (this.registerUserForm.valid) {
			const newUser: UserDTO = this.registerUserForm.value;
			console.log(newUser);
			this._authService.registerUser(newUser);
		} else {
			console.log('Form is invalid');
		}
	}
}