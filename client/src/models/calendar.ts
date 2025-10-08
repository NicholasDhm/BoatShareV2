import { DateTime } from "luxon";
import { IReservation } from "./reservation";

export interface ICalendarViewModel {
  currentDate: DateTime;
  displayDate: DateTime;
  occupiedDates: IReservation[];
	currentUserReservations: IReservation[];
}

export interface IDay {
	date: number;
	month: number;
	year: number;
	isCurrentMonth: boolean;
	state: string; // Standard | Substitution | Contingency
	status?: 'Pending' | 'Unconfirmed' | 'Confirmed';
}