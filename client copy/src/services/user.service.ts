import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IUser } from '../models/user';
import { BehaviorSubject, Observable } from 'rxjs';
import { IReservation } from '../models/reservation';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class UserService {	
  private baseUrl = `${environment.baseUrl}users`;
	
  private _currentUserSubject = new BehaviorSubject<IUser | null>(null);
  currentUser$ = this._currentUserSubject.asObservable();

  setCurrentUser(user: IUser): void {
    this._currentUserSubject.next(user);
		localStorage.setItem('currentUser', JSON.stringify(user)); // Persist user in local storage
  }

  getCurrentUser(): IUser | null {
    return this._currentUserSubject.value;
  }

	clearCurrentUser(): void {
		this._currentUserSubject.next(null);
	}

	updateCurrentUser(): void {
		const userId = this.getCurrentUser()?.userId;
		this._httpClient.get<IUser>(`${this.baseUrl}/${userId}`).subscribe(updatedUser => {
			this._currentUserSubject.next(updatedUser);
			this.setCurrentUser(updatedUser); // Refresh the user information
		});		
	}

  constructor(
    private _httpClient: HttpClient
  ) { }

  // Method to get users by partial name
  getUsersByPartialName(partialName: string): Observable<IUser[]> {
    return this._httpClient.get<IUser[]>(`${this.baseUrl}/search/${partialName}`);
  }
	
	updateUser(user: IUser): Promise<void> {
    return new Promise((resolve, reject) => {
			this._httpClient.put<IUser>(`${this.baseUrl}/${user.userId}`, user).subscribe(
				(updatedUser) => {
					console.log('User updated successfully:', updatedUser);
					resolve();
				},
				(error) => {
					console.error('Error updating user:', error);
					reject(error);
				}
			);
		});
	}

  getAllUsers(): Promise<IUser[]> {
    return new Promise((resolve, reject) => {
      this._httpClient.get<IUser[]>(this.baseUrl).subscribe({
        next: result => {
          console.debug(`getUsers: url ${this.baseUrl} result`, result);
          resolve(result);
        },
        error: err => {
          console.warn(`getUsers: url ${this.baseUrl}`, err);
          reject(err);
        }
      });
    });
  }
	
	getUserById(userId: string): Promise<IUser> {
		const url = this.baseUrl + `/${userId}`;
		return new Promise((resolve, reject) => {
			this._httpClient.get<IUser>(url).subscribe({
				next: result => {
					console.debug(`getUserById: url ${url} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`getUserById: url ${url}`, err);
					reject(err);
				}
			});
		});
	}

	deleteUserById(userId: string): Promise<IUser> {
		const url = this.baseUrl + `/${userId}`;
		return new Promise((resolve, reject) => {
			this._httpClient.delete<IUser>(url).subscribe({
				next: result => {
					console.debug(`deleteUserById: url ${url} result`, result);
					resolve(result);
				},
				error: err => {
					console.warn(`deleteUserById: url ${url}`, err);
					reject(err);
				}
			});
		});
	}

	addQuotasBack(userId: string, reservation: IReservation): Promise<IUser> {
    return new Promise((resolve, reject) => {
      const url = `${this.baseUrl}/${userId}/add-quotas`;
      this._httpClient.put<IUser>(url, reservation).subscribe({
        next: result => {
          console.debug(`addQuotasBack: url ${url} result`, result);
					this.updateCurrentUser(); // Refresh the user information
          resolve(result);
        },
        error: err => {
          console.warn(`addQuotasBack: url ${url}`, err);
          reject(err);
        }
      });
    });
  }
}