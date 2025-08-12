import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { HomeComponent } from '../pages/home/home.component';
import { DashboardComponent } from '../pages/dashboard/dashboard.component';
import { ManageUsersComponent } from '../pages/manage-users/mange-users.component';
import { ProfileComponent } from '../pages/profile/profile.component';
import { ManageBoatsComponent } from '../pages/manage-boats/manage-boats.component';
import { LoginComponent } from '../pages/login/login.component';
import { UiCalendarComponent } from '../components/ui-calendar/ui-calendar.component';
import { TestingPageComponent } from '../testing-page/testing-page.component';
import { RegisterUserComponent } from '../pages/register-user/register.component';

export const routes: Routes = [
  {
    path: 'home',
    component: HomeComponent
	},
  {
    path: 'dashboard',
    component: DashboardComponent
	},
	{
		path: 'profile',
		component: ProfileComponent
	},
	{
		path: 'manage-users',
		component: ManageUsersComponent
	},
	{
		path: 'manage-boats',
		component: ManageBoatsComponent
	},
  {
		path: 'login',
		component: LoginComponent
	},
  {
		path: 'register-user',
		component: RegisterUserComponent
	},
  {
		path: 'test-page',
		component: TestingPageComponent 
	},
  {
		path: 'calendar',
		component: UiCalendarComponent 
	},
  {
		path: '**',
		redirectTo: 'dashboard',
		pathMatch: 'full'
	}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
