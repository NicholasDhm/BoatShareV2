import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { AuthService } from '../../auth/auth.service';

interface LoginForm {
  email: string;
  password: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  
  private readonly destroy$ = new Subject<void>();

  constructor(
    private readonly authService: AuthService,
    private readonly formBuilder: FormBuilder,
  ) {
    this.loginForm = this.createLoginForm();
  }

  ngOnInit(): void {
    // Auto-focus email field after component loads
    setTimeout(() => {
      const emailInput = document.getElementById('email');
      emailInput?.focus();
    }, 100);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private createLoginForm(): FormGroup {
    return this.formBuilder.group({
      email: ['', [
        Validators.required, 
        Validators.email,
        Validators.maxLength(255)
      ]],
      password: ['', [
        Validators.required, 
        Validators.minLength(6),
        Validators.maxLength(128)
      ]],
    });
  }

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    try {
      const formValue = this.loginForm.value as LoginForm;
      await this.authService.login(formValue.email, formValue.password);
    } catch (error) {
      this.errorMessage = this.getErrorMessage(error);
    } finally {
      this.isLoading = false;
    }
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
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
  get email() { return this.loginForm.get('email'); }
  get password() { return this.loginForm.get('password'); }

  get isEmailInvalid(): boolean {
    return !!(this.email?.invalid && this.email?.touched);
  }

  get isPasswordInvalid(): boolean {
    return !!(this.password?.invalid && this.password?.touched);
  }

  get emailErrorMessage(): string {
    if (this.email?.errors?.['required']) {
      return 'Email is required';
    }
    if (this.email?.errors?.['email']) {
      return 'Please enter a valid email address';
    }
    if (this.email?.errors?.['maxlength']) {
      return 'Email is too long';
    }
    return '';
  }

  get passwordErrorMessage(): string {
    if (this.password?.errors?.['required']) {
      return 'Password is required';
    }
    if (this.password?.errors?.['minlength']) {
      return 'Password must be at least 6 characters';
    }
    if (this.password?.errors?.['maxlength']) {
      return 'Password is too long';
    }
    return '';
  }
}