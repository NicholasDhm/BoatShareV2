import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IDay } from '../../models/calendar';

const TYPE_LABELS: Record<string, string> = {
	Standard: 'padrão',
	Substitution: 'suplência',
	Contingency: 'contingência'
};

@Component({
	selector: 'ui-calendar-day',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-calendar-day.component.html',
	styleUrls: ['./ui-calendar-day.component.scss']
})
export class UiCalendarDayComponent {
	@Input() day: IDay | null = null;
	@Output() daySelected = new EventEmitter<void>();

	get isSelectable(): boolean {
		return !!this.day && this.day.isCurrentMonth && this.day.state !== 'Greyish';
	}

	get statusTone(): string {
		switch (this.day?.status) {
			case 'Confirmed': return 'good';
			case 'Pending': return 'warning';
			case 'Unconfirmed': return 'critical';
			default: return 'neutral';
		}
	}

	get statusIcon(): string {
		switch (this.day?.status) {
			case 'Confirmed': return 'bi-check-lg';
			case 'Pending': return 'bi-hourglass-split';
			case 'Unconfirmed': return 'bi-exclamation-lg';
			default: return 'bi-dot';
		}
	}

	get ariaLabel(): string {
		if (!this.day) {
			return '';
		}
		const type = TYPE_LABELS[this.day.state] ?? '';
		return `Dia ${this.day.date}${type ? ', reserva ' + type : ''}`;
	}

	onDaySelected(): void {
		if (this.isSelectable) {
			this.daySelected.emit();
		}
	}
}
