export interface IReservation {
	reservationId?: string;
	status?: 'Pending' | 'Confirmed' | 'Unconfirmed';
	createdAtIsoDate?: string;
	
	type: ReservationType;
	userId: string;
	boatId: string;
	year: number;
	month: number;
	day: number;
}

export type ReservationType = 'Standard' | 'Substitution' | 'Contingency';