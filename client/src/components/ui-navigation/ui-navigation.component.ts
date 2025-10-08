import { Component, Input } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'ui-navigation',
  standalone: true,
  imports: [
    RouterModule,
  ],
  templateUrl: './ui-navigation.component.html',
  styleUrls: ['./ui-navigation.component.scss']
})
export class UiNavigationComponent {
  @Input() role: 'Admin' | 'Member' | 'Unknown' = 'Member';
}