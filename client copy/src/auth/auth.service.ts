import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../environments/environment';
import { UserService } from '../services/user.service';
import { IUser, UserDTO } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.baseUrl}Auth`;

  constructor(
    private _httpClient: HttpClient,
    private _router: Router,
    private _userService: UserService,
  ) {}

  login(email: string, password: string) {
    return this._httpClient.post<{ token: string; user: IUser }>(`${this.apiUrl}/login`, { email, password })
      .subscribe(response => {
        const token = response.token; // Get the token from the response
        localStorage.setItem('authToken', token); // Store token in localStorage
  
        const userViewModel: IUser = {
          userId: response.user.userId, // Access user properties from the response
          name: response.user.name,
          email: response.user.email,
          role: response.user.role,
          standardQuota: response.user.standardQuota,
          substitutionQuota: response.user.substitutionQuota,
          contingencyQuota: response.user.contingencyQuota,
          boatId: response.user.boatId
        };
        this._userService.setCurrentUser(userViewModel);
  
        this._router.navigate(['/dashboard']); // Redirect after login
      }, error => {
        console.error('Login failed', error);
      });
  }

  logout() {
    localStorage.removeItem('authToken'); // Clear token from localStorage
    this._userService.clearCurrentUser(); // Clear user data in UserService
    this._router.navigate(['/login']); // Redirect to login page
  }

  initializeSession(): void {
    const token = localStorage.getItem('authToken');
    if (token) {
      // Restore user data from localStorage if available
      const user = localStorage.getItem('currentUser');
      if (user) {
        this._userService.setCurrentUser(JSON.parse(user));
      }
      // Optionally set the token in a service if necessary
    } else {
      this._router.navigate(['/login']);
    }
  }

  registerUser(userDTO: UserDTO): Promise<string> {
    return new Promise((resolve, reject) => {
      this._httpClient.post<string>(`${this.apiUrl}/register`, userDTO, { responseType: 'text' as 'json' })
        .subscribe({
          next: async response => {
            console.debug(`User registered`);
            resolve(response);
  
            // Call the login method after registration
            try {
              await this.login(userDTO.email, userDTO.password); // Assuming password is accessible
            } catch (error) {
              console.error('Login failed after registration', error);
            }
          },
          error: err => {
            console.warn(`Failed to register user`, err);
            reject(err);
          }
        });
    });
  }

  updateUserPassword(userId: string, newPassword: string): Promise<void> {
    return new Promise((resolve, reject) => {
      this._httpClient.put<void>(`${this.apiUrl}/new-password`, { userId, newPassword })
        .subscribe({
          next: () => resolve(),
          error: (err) => {
            console.warn(`Failed to update password for user ${userId}. Error:`, err);
            reject(err);
          }
        });
    });
  }
  
}

