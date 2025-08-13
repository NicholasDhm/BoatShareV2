import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { TestingPageComponent } from '../testing-page/testing-page.component';
import { UiNavigationComponent } from '../components/ui-navigation/ui-navigation.component';
import { UiCardComponent } from '../components/ui-card/ui-card.component';
import { LogoutComponent } from '../pages/logout/logout.component';
import { UiLoadingSpinnerComponent } from '../components/ui-loading-spinner/ui-loading-spinner.component';

import { IUser } from '../models/user';
import { AuthService } from '../auth/auth.service';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
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
export class AppComponent implements OnInit, OnDestroy {
  readonly title = 'BoatShare v2';
  activeUser: IUser | null = null;
  
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly authService: AuthService,
    private readonly userService: UserService,
  ) {
    this.authService.initializeSession();
    this.activeUser = this.userService.getCurrentUser();
  }

  ngOnInit(): void {
    this.userService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user: IUser | null) => {
          this.activeUser = user;
        },
        error: (error: unknown) => {
          console.error('Error loading user:', error);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get userRole(): IUser['role'] | 'Unknown' {
    return this.activeUser?.role ?? 'Unknown';
  }

  get userName(): string {
    return this.activeUser?.name ?? '';
  }
}
