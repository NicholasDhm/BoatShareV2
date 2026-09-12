import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';

export type StatTone = 'accent' | 'good' | 'warning' | 'critical' | 'neutral';

@Component({
	selector: 'ui-stat-tile',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-stat-tile.component.html',
	styleUrls: ['./ui-stat-tile.component.scss']
})
export class UiStatTileComponent implements OnChanges {
	@Input() label = '';
	@Input() value: string | number = 0;
	@Input() unit = '';
	@Input() hint = '';
	@Input() icon = 'bi-graph-up';
	@Input() tone: StatTone = 'accent';
	/** Optional 12-point trend; the last point is the current period. */
	@Input() trend: number[] | null = null;

	linePath = '';
	endX = 0;
	endY = 0;

	ngOnChanges(): void {
		if (!this.trend || this.trend.length < 2) {
			this.linePath = '';
			return;
		}

		const width = 120;
		const height = 32;
		const max = Math.max(...this.trend);
		const min = Math.min(...this.trend);
		const span = max - min || 1;
		const stepX = width / (this.trend.length - 1);

		const points = this.trend.map((value, index) => ({
			x: index * stepX,
			y: height - 4 - ((value - min) / span) * (height - 8)
		}));

		this.linePath = points.map((point, index) => `${index === 0 ? 'M' : 'L'}${point.x.toFixed(1)},${point.y.toFixed(1)}`).join(' ');
		this.endX = points[points.length - 1].x;
		this.endY = points[points.length - 1].y;
	}
}
