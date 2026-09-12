import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IChartSlice } from '../../../models/chart';

interface Arc extends IChartSlice {
	dash: number;
	offset: number;
	share: number;
}

const RADIUS = 62;
const GAP = 2; // surface gap between adjacent segments

@Component({
	selector: 'ui-donut-chart',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-donut-chart.component.html',
	styleUrls: ['./ui-donut-chart.component.scss']
})
export class UiDonutChartComponent implements OnChanges {
	@Input() slices: IChartSlice[] = [];
	@Input() caption = '';
	/** Overrides the centre figure when the total is not the story. */
	@Input() total: number | null = null;

	readonly radius = RADIUS;
	readonly circumference = 2 * Math.PI * RADIUS;

	arcs: Arc[] = [];
	sum = 0;
	hovered: number | null = null;

	ngOnChanges(): void {
		this.sum = this.slices.reduce((acc, slice) => acc + Math.max(0, slice.value), 0);
		let cursor = 0;

		this.arcs = this.slices.map(slice => {
			const value = Math.max(0, slice.value);
			const share = this.sum > 0 ? value / this.sum : 0;
			const full = share * this.circumference;
			const arc: Arc = {
				...slice,
				share,
				dash: Math.max(0, full - (value > 0 && this.sum !== value ? GAP : 0)),
				offset: -cursor
			};
			cursor += full;
			return arc;
		});
	}

	get centreValue(): string {
		if (this.hovered !== null) {
			return `${this.arcs[this.hovered].value}`;
		}
		return `${this.total ?? this.sum}`;
	}

	get centreCaption(): string {
		return this.hovered !== null ? this.arcs[this.hovered].label : this.caption;
	}

	percent(share: number): string {
		return `${Math.round(share * 100)}%`;
	}
}
