import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IChartBar } from '../../../models/chart';

interface Column extends IChartBar {
	path: string;
	labelX: number;
	capY: number;
	hitX: number;
	hitWidth: number;
	isPeak: boolean;
}

const VIEW_W = 720;
const VIEW_H = 250;
const PAD = { top: 18, right: 10, bottom: 30, left: 36 };
const MAX_BAR = 24;
const RADIUS = 4;

@Component({
	selector: 'ui-bar-chart',
	standalone: true,
	imports: [CommonModule],
	templateUrl: './ui-bar-chart.component.html',
	styleUrls: ['./ui-bar-chart.component.scss']
})
export class UiBarChartComponent implements OnChanges {
	@Input() bars: IChartBar[] = [];
	/** CSS colour for the single series. */
	@Input() color = 'var(--series-standard)';
	@Input() valueSuffix = '';

	readonly viewBox = `0 0 ${VIEW_W} ${VIEW_H}`;
	readonly plotLeft = PAD.left;
	readonly plotRight = VIEW_W - PAD.right;
	readonly baseline = VIEW_H - PAD.bottom;

	columns: Column[] = [];
	ticks: { value: number; y: number }[] = [];
	hovered: number | null = null;

	ngOnChanges(): void {
		const plotW = VIEW_W - PAD.left - PAD.right;
		const plotH = VIEW_H - PAD.top - PAD.bottom;
		const peak = Math.max(0, ...this.bars.map(bar => bar.value));
		const { max, step } = this.niceScale(peak);

		this.ticks = [];
		for (let value = 0; value <= max + step / 2; value += step) {
			const rounded = Math.round(value * 100) / 100;
			this.ticks.push({ value: rounded, y: this.baseline - (rounded / max) * plotH });
		}

		const band = this.bars.length > 0 ? plotW / this.bars.length : plotW;
		const barW = Math.max(6, Math.min(MAX_BAR, band - 12));

		this.columns = this.bars.map((bar, index) => {
			const centre = PAD.left + band * index + band / 2;
			const x = centre - barW / 2;
			const height = max > 0 ? (Math.max(0, bar.value) / max) * plotH : 0;
			const y = this.baseline - height;

			return {
				...bar,
				path: this.barPath(x, y, barW, height),
				labelX: centre,
				capY: y,
				hitX: PAD.left + band * index + 1,
				hitWidth: Math.max(1, band - 2),
				isPeak: bar.value === peak && peak > 0
			};
		});
	}

	tooltipLeft(column: Column): string {
		return `${(column.labelX / VIEW_W) * 100}%`;
	}

	tooltipTop(column: Column): string {
		return `${(column.capY / VIEW_H) * 100}%`;
	}

	/** Tall bars leave no room above the cap — flip the tooltip below it. */
	tooltipFlipped(column: Column): boolean {
		return column.capY < 70;
	}

	/** Bars are rounded at the data end and square where they meet the baseline. */
	private barPath(x: number, y: number, width: number, height: number): string {
		if (height <= 0) {
			return '';
		}
		const r = Math.min(RADIUS, height, width / 2);
		return [
			`M${x},${y + height}`,
			`V${y + r}`,
			`Q${x},${y} ${x + r},${y}`,
			`H${x + width - r}`,
			`Q${x + width},${y} ${x + width},${y + r}`,
			`V${y + height}`,
			'Z'
		].join(' ');
	}

	/** Ticks land on round numbers: pick a nice step first, then snap the top to it. */
	private niceScale(peak: number): { max: number; step: number } {
		if (peak <= 0) {
			return { max: 4, step: 1 };
		}

		const rawStep = peak / 4;
		const magnitude = Math.pow(10, Math.floor(Math.log10(rawStep)));
		const normalised = rawStep / magnitude;
		const step = Math.max(
			magnitude * (normalised <= 1 ? 1 : normalised <= 2 ? 2 : normalised <= 5 ? 5 : 10),
			peak <= 4 ? 1 : 0
		);

		return { max: Math.ceil(peak / step) * step, step };
	}
}
