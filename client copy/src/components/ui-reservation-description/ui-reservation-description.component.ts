import { Component, AfterViewInit, Input } from '@angular/core';
import { UiCardComponent } from '../ui-card/ui-card.component';
import { CommonModule } from '@angular/common';
import { UiSvgComponent } from '../ui-svg/ui-svg.component';
import * as bootstrap from 'bootstrap';
import { IUser } from '../../models/user';

@Component({
	selector: 'ui-reservation-description',
	standalone: true,
	imports: [
		UiCardComponent,
		CommonModule,
		UiSvgComponent
	],
	templateUrl: './ui-reservation-description.component.html',
	styleUrls: ['./ui-reservation-description.component.scss']
})
export class UiReservationDescriptionComponent implements AfterViewInit {
	@Input() user: IUser | null = null;

	ngAfterViewInit() {
		const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle$="-state-popover"]'));
		popoverTriggerList.map(function (popoverTriggerEl) {
			return new bootstrap.Popover(popoverTriggerEl, {
				trigger: 'focus',
   			html: true
			});
		});
	}
}