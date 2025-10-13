import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { IBoat } from '../../models/boat';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { BoatService } from '../../services/boat.service';
import { UserDTO } from '../../models/user';
import { Subscription } from 'rxjs';
import { AuthService } from '../../auth/auth.service';

@Component({
	selector: 'app-register-user',
	standalone: true,
	imports: [CommonModule, RouterModule, ReactiveFormsModule],
	templateUrl: './register-user.component.html',
	styleUrls: ['./register-user.component.scss']
})
export class RegisterUserComponent implements OnInit {
  private _subscriptions: Subscription = new Subscription();
	registerUserForm: FormGroup;
	boats: IBoat[] = [];
	isLoading = false;
	errorMessage = '';

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

	async registerNewUser(): Promise<void> {
		if (this.registerUserForm.invalid) {
			this.markFormGroupTouched();
			return;
		}

		this.isLoading = true;
		this.errorMessage = '';

		try {
			const newUser: UserDTO = this.registerUserForm.value;
			await this._authService.registerUser(newUser);
		} catch (error) {
			this.errorMessage = this.getErrorMessage(error);
		} finally {
			this.isLoading = false;
		}
	}

	private markFormGroupTouched(): void {
		Object.keys(this.registerUserForm.controls).forEach(key => {
			const control = this.registerUserForm.get(key);
			control?.markAsTouched();
		});
	}

	private getErrorMessage(error: unknown): string {
		if (error instanceof Error) {
			return error.message;
		}
		return 'An unexpected error occurred. Please try again.';
	}

	// Getters for template
	get name() { return this.registerUserForm.get('name'); }
	get email() { return this.registerUserForm.get('email'); }
	get boatId() { return this.registerUserForm.get('boatId'); }
	get password() { return this.registerUserForm.get('password'); }

	get isNameInvalid(): boolean {
		return !!(this.name?.invalid && this.name?.touched);
	}

	get isEmailInvalid(): boolean {
		return !!(this.email?.invalid && this.email?.touched);
	}

	get isBoatIdInvalid(): boolean {
		return !!(this.boatId?.invalid && this.boatId?.touched);
	}

	get isPasswordInvalid(): boolean {
		return !!(this.password?.invalid && this.password?.touched);
	}

	get nameErrorMessage(): string {
		if (this.name?.errors?.['required']) {
			return 'Name is required';
		}
		return '';
	}

	get emailErrorMessage(): string {
		if (this.email?.errors?.['required']) {
			return 'Email is required';
		}
		if (this.email?.errors?.['email']) {
			return 'Please enter a valid email address';
		}
		return '';
	}

	get boatIdErrorMessage(): string {
		if (this.boatId?.errors?.['required']) {
			return 'Please select a boat';
		}
		return '';
	}

	get passwordErrorMessage(): string {
		if (this.password?.errors?.['required']) {
			return 'Password is required';
		}
		return '';
	}

	getAvailableBoats(): IBoat[] {
		return this.boats.filter(boat => boat.assignedUsersCount < boat.capacity);
	}
}