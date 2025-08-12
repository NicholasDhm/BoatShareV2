import { Injectable, Signal, signal } from '@angular/core';
import { UserService } from '../../services/user.service';
import { IUser } from '../../models/user';
import { IReservation } from '../../models/reservation';

export interface IUserTestingViewModel {
	getUserId: string;

	postUserId: string;
	postUserName: string;
	postUserEmail: string;
	postUserRole: 'Member' | 'Admin';
	postUserQuotas: number;
	postUserBoatId: string;

	deleteUserId: string;
	
	searchedUsers: IUser[];
	searchQuery: string;
}

@Injectable({
	providedIn: 'root'
})
export class UserTestPageService {
	currentUser: IUser | null = null;

	_viewModel = signal<IUserTestingViewModel>({
		getUserId: "1",

		postUserId: "1",
		postUserName: 'name',
		postUserEmail: 'email@gmail.com',
		postUserRole: 'Member',
		postUserQuotas: 8,
		postUserBoatId: '1',

		deleteUserId: "1",

		searchedUsers: [],
		searchQuery: '',
	});

	constructor(
		private _userService: UserService,
	) {
		this.currentUser = this._userService.getCurrentUser();
	}

	get viewModel(): Signal<IUserTestingViewModel> {
		return this._viewModel;
	}

	updateViewModel(value: string | number | IUser[], type: keyof IUserTestingViewModel): void {
		this._viewModel.set(Object.assign({}, this._viewModel(), { [type]: value }));
	}

	// Method to search users by partial name
	searchUsers(): void {
	  this._userService.getUsersByPartialName(this._viewModel().searchQuery).subscribe(
		(result: IUser[]) => {
		  this.updateViewModel(result, 'searchedUsers');
		},
		(error) => {
		  console.error('Error fetching users', error);
		}
	  );
	}

	getAllUsers(): Promise<IUser[]> {
		return this._userService.getAllUsers().then(users => {
			return users;
		})
	}

	getUserById(): Promise<IUser> {
		return this._userService.getUserById(this._viewModel().getUserId).then(user => {
			return user;
		})
	}

	addQuota(): Promise<IUser> {
		const randomReservation: IReservation = {
			userId: this.currentUser?.userId || '',
			boatId: '1',
			year: 2024,
			month: 10,
			day: 26,
			type: 'Standard',
		}

		return this._userService.addQuotasBack(randomReservation.userId, randomReservation).then(user => {
			return user;
		});
	}

	// postUser(): Promise<IUser> {
	// 	const userModel: IUser = {
	// 		userId: this._viewModel().postUserId,
	// 		userName: this._viewModel().postUserName,
	// 		userEmail: this._viewModel().postUserEmail,
	// 		userRole: this._viewModel().postUserRole,
	// 		userQuota: this._viewModel().postUserQuota,
	// 		boatId: this._viewModel().postUserBoatId,
	// 	};

	// 	return this._userService.postUser(userModel).then(user => {
	// 		return user;
	// 	});
	// }

	// postRegularUser(): Promise<string> {
	// 	const userModel: UserDTO = {
	// 		name: this._viewModel().postUserName,
	// 		email: this._viewModel().postUserEmail,
	// 		boatId: this._viewModel().postUserBoatId,
	// 	};

	// 	return this._userService.postRegularUser(userModel).then(user => {
	// 		return user;
	// 	});
	// }

	// postUserAsAdmin(): Promise<IUser> {
	// 	const userModel: AdminUserDTO = {
	// 		name: this._viewModel().postUserName,
	// 		email: this._viewModel().postUserEmail,
	// 		role: this._viewModel().postUserRole,
	// 		quotas: this._viewModel().postUserQuotas,
	// 		boatId: this._viewModel().postUserBoatId,
	// 	};

	// 	return this._userService.postUserAsAdmin(userModel).then(user => {
	// 		return user;
	// 	});
	// }

	deleteUserById(): Promise<IUser> {
		return this._userService.deleteUserById(this._viewModel().deleteUserId).then(user => {
			return user;
		});
	}

}