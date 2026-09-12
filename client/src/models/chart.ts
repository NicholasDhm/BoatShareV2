export interface IChartSlice {
	label: string;
	value: number;
	/** CSS colour — pass a token, e.g. `var(--series-standard)`. */
	color: string;
}

export interface IChartBar {
	label: string;
	/** Longer label used by the tooltip when `label` is abbreviated. */
	fullLabel?: string;
	value: number;
}
