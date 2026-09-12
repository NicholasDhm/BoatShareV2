import { Component, ElementRef, HostListener, Input } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { UiThemeToggleComponent } from '../ui-theme-toggle/ui-theme-toggle.component';
import { AuthService } from '../../auth/auth.service';

@Component({
	selector: 'ui-navigation',
	standalone: true,
	imports: [RouterModule, CommonModule, UiThemeToggleComponent],
	templateUrl: './ui-navigation.component.html',
	styleUrls: ['./ui-navigation.component.scss']
})
export class UiNavigationComponent {
	@Input() role: 'Admin' | 'Member' | 'Unknown' = 'Member';
	@Input() userName = '';
	@Input() boatName = '';

	isMenuOpen = false;
	isUserMenuOpen = false;

	constructor(
		private readonly _elementRef: ElementRef<HTMLElement>,
		private readonly _authService: AuthService,
	) {}

	get isAuthenticated(): boolean {
		return this.role !== 'Unknown';
	}

	get roleLabel(): string {
		return this.role === 'Admin' ? 'Administrador' : 'Membro';
	}

	get initials(): string {
		return this.userName
			.split(' ')
			.filter(part => part.length > 0)
			.slice(0, 2)
			.map(part => part[0].toUpperCase())
			.join('');
	}

	toggleMenu(): void {
		this.isMenuOpen = !this.isMenuOpen;
		this.isUserMenuOpen = false;
	}

	toggleUserMenu(): void {
		this.isUserMenuOpen = !this.isUserMenuOpen;
	}

	closeAll(): void {
		this.isMenuOpen = false;
		this.isUserMenuOpen = false;
	}

	logout(): void {
		this.closeAll();
		this._authService.logout();
	}

	@HostListener('document:click', ['$event'])
	onDocumentClick(event: MouseEvent): void {
		if (!this._elementRef.nativeElement.contains(event.target as Node)) {
			this.closeAll();
		}
	}

	@HostListener('document:keydown.escape')
	onEscape(): void {
		this.closeAll();
	}
}
