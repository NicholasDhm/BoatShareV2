import { Component } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';

@Component({
	selector: 'app-login',
	standalone: true,
	imports: [UiCardComponent, RouterModule, ReactiveFormsModule],
	templateUrl: './login.component.html',
	styleUrls: ['./login.component.scss']
})
export class LoginComponent {
	createAccountForm: FormGroup;

  constructor(
		private _authService: AuthService,
		private fb: FormBuilder,
	) {
		this.createAccountForm = this.fb.group({
			email: ['', [Validators.required, Validators.email]],
			password: ['', [Validators.required, Validators.minLength(6)]],
		});
	}

  verifyAndLogin() {
		if (this.createAccountForm.valid) {
			this._authService.login(this.createAccountForm.value.email, this.createAccountForm.value.password);
		} else {
			console.log('Form is invalid');
		}
  }
}