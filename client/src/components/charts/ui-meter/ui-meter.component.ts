import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
	selector: 'ui-meter',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-meter.component.html',
	styleUrls: ['./ui-meter.component.scss']
})
export class UiMeterComponent {
	@Input() label = '';
	@Input() value = 0;
	@Input() max = 0;
	@Input() color = 'var(--series-standard)';
	@Input() hint = '';

	get percent(): number {
		if (this.max <= 0) {
			return 0;
		}
		return Math.min(100, Math.round((this.value / this.max) * 100));
	}
}
