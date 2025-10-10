export interface IReservation {
	reservationId?: number;
	status?: 'Pending' | 'Confirmed' | 'Unconfirmed';
	createdAtIsoDate?: string;
	userName?: string;

	type: ReservationType;
	userId: number;
	boatId: number;
	year: number;
	month: number;
	day: number;
}

export type ReservationType = 'Standard' | 'Substitution' | 'Contingency';