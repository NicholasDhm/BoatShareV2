import { Component, Input } from '@angular/core';

@Component({
  selector: 'ui-svg',
  standalone: true,
  templateUrl: './ui-svg.component.html',
  styleUrls: ['./ui-svg.component.scss']
})
export class UiSvgComponent {
  @Input() state: string = 'Standard';
  @Input() type: 'Confirmed' | 'Pending' | 'Unconfirmed' | 'boat' = 'boat';
  @Input() width: number = 14;
  @Input() height: number = 14;
}