import { Component } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';

@Component({
	selector: 'app-logout',
	standalone: true,
	imports: [UiCardComponent],
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