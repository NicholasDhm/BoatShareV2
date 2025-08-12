import { Component } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TestingPageComponent } from '../testing-page/testing-page.component';
import { UiNavigationComponent } from '../components/ui-navigation/ui-navigation.component';
import { UiCardComponent } from '../components/ui-card/ui-card.component';
import { LogoutComponent } from '../pages/logout/logout.component';
import { IUser } from '../models/user';
import { AuthService } from '../auth/auth.service';
import { UserService } from '../services/user.service';
import { UiLoadingSpinnerComponent } from '../components/ui-loading-spinner/ui-loading-spinner.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterModule,
    TestingPageComponent,
    UiNavigationComponent,
    UiCardComponent,
    LogoutComponent,
    UiLoadingSpinnerComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'boat-share';
  activeUser: IUser | null = null;

  constructor(
    private _authService: AuthService,
    private _userService: UserService,
  ) {
    this._authService.initializeSession();
		this.activeUser = this._userService.getCurrentUser();
  }

	ngOnInit(): void {
    this._userService.currentUser$.subscribe(user => {
      this.activeUser = user;
    });
  }
}
