import { Component } from '@angular/core';
import { AuthService } from '../../auth/auth.service';

@Component({
	selector: 'app-logout',
	standalone: true,
	imports: [],
	templateUrl: './logout.component.html',
	styleUrls: ['./logout.component.scss']
})
export class LogoutComponent {

  constructor(
    private _authService: AuthService,
  ) {}

  logout() {
    this._authService.logout();
  }
}