import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UiSvgComponent } from '../ui-svg/ui-svg.component';
import { IDay } from '../../models/calendar';

@Component({
	selector: 'ui-calendar-day',
	standalone: true,
	imports: [CommonModule, UiSvgComponent],
	templateUrl: './ui-calendar-day.component.html',
	styleUrls: ['./ui-calendar-day.component.scss']
})
export class UiCalendarDayComponent {
	@Input() day: IDay | null = null;
	@Output() daySelected = new EventEmitter<void>();

	onDaySelected(): void {
		this.daySelected.emit();
	}
}