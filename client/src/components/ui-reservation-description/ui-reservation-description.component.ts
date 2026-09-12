import { Component, AfterViewInit, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as bootstrap from 'bootstrap';
import { IUser } from '../../models/user';

interface QuotaCard {
	key: 'standard' | 'substitution' | 'contingency';
	label: string;
	title: string;
	content: string;
	value: number;
}

@Component({
	selector: 'ui-reservation-description',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-reservation-description.component.html',
	styleUrls: ['./ui-reservation-description.component.scss']
})
export class UiReservationDescriptionComponent implements AfterViewInit, OnChanges {
	@Input() user: IUser | null = null;

	quotaCards: QuotaCard[] = [];

	ngOnChanges(): void {
		if (!this.user) {
			this.quotaCards = [];
			return;
		}

		this.quotaCards = [
			{
				key: 'standard',
				label: 'Padrão',
				title: 'Reserva — Padrão',
				value: this.user.standardQuota,
				content: '• Consome 1 cota padrão.<br/>• Pode ser feita em qualquer dia livre do calendário.<br/>• Se a reserva for cancelada ou o dia passar, a cota volta automaticamente.'
			},
			{
				key: 'substitution',
				label: 'Suplência',
				title: 'Reserva — Suplência',
				value: this.user.substitutionQuota,
				content: '• Consome 1 cota suplente.<br/>• Pode ser feita em um dia já reservado por outro membro.<br/>• Se o titular cancelar, a data passa a ser sua; se o dia passar, a cota volta.'
			},
			{
				key: 'contingency',
				label: 'Contingência',
				title: 'Reserva — Contingência',
				value: this.user.contingencyQuota,
				content: '• Não consome cota padrão.<br/>• Só pode ser feita no mesmo dia, a partir das 6:00.<br/>• Vale para dias em aberto, sem conflito com outras reservas.'
			}
		];
	}

	ngAfterViewInit(): void {
		const triggers = Array.from(document.querySelectorAll('[data-bs-toggle$="-state-popover"]'));
		triggers.forEach(trigger => new bootstrap.Popover(trigger, { trigger: 'focus', html: true }));
	}
}
