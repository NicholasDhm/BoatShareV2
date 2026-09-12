import { Component } from '@angular/core';
import { ThemeService } from '../../services/theme.service';

@Component({
  selector: 'ui-theme-toggle',
  standalone: true,
  template: `
    <button
      type="button"
      class="theme-toggle"
      (click)="themeService.toggle()"
      [attr.aria-label]="themeService.theme() === 'dark' ? 'Ativar tema claro' : 'Ativar tema escuro'"
      [title]="themeService.theme() === 'dark' ? 'Tema claro' : 'Tema escuro'">
      <i class="bi" [class.bi-sun]="themeService.theme() === 'dark'" [class.bi-moon-stars]="themeService.theme() === 'light'"></i>
    </button>
  `,
  styles: [`
    .theme-toggle {
      display: grid;
      place-items: center;
      width: 40px;
      height: 40px;
      color: var(--ink-2);
      background: var(--surface-2);
      border: 1px solid var(--stroke);
      border-radius: 50%;
      cursor: pointer;
      transition: color var(--t-fast), border-color var(--t-fast), transform var(--t-fast);
    }
    .theme-toggle:hover {
      color: var(--accent);
      border-color: var(--stroke-brand);
      transform: rotate(-18deg);
    }
    .theme-toggle i { font-size: 1rem; }
  `]
})
export class UiThemeToggleComponent {
  constructor(public themeService: ThemeService) {}
}
