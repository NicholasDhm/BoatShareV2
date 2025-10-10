import { Component, EventEmitter, Input, Output, SimpleChanges } from '@angular/core';
import { UiCardComponent } from '../ui-card/ui-card.component';
import { DateTime } from 'luxon';
import { CommonModule } from '@angular/common';
import { ICalendarViewModel, IDay } from '../../models/calendar';
import { UiCalendarDayComponent } from "../ui-calendar-day/ui-calendar-day.component";

@Component({
	selector: 'ui-calendar',
	standalone: true,
	imports: [UiCardComponent, CommonModule, UiCalendarDayComponent],
	templateUrl: './ui-calendar.component.html',
	styleUrls: ['./ui-calendar.component.scss']
})
export class UiCalendarComponent {
	@Input() calendarViewModel: ICalendarViewModel | null = null;
  @Output() selectedDay = new EventEmitter<IDay>();
	@Output() monthChanged = new EventEmitter<number>();

	monthToDisplay: string = '';

	daysOfWeek: string[] = [ 'Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb' ];
	months: string[] = [ 'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro' ];
	weeksOfMonth: IDay[][] | null = null;

	ngOnChanges(changes: SimpleChanges): void {
		if (changes['calendarViewModel'] && this.calendarViewModel) {
			this.populateDaysOfMonth(this.calendarViewModel.displayDate.year, this.calendarViewModel.displayDate.month);
		}
	}

	onDateSelected(day: IDay): void {
		this.selectedDay.emit(day);
	}

	monthClick(direction: number): void {
		this.monthChanged.emit(direction);
	}

	populateDaysOfMonth(year: number, month: number): void {
		// Get the number of days in the current and previous month
		const daysInCurrentMonth = new Date(year, month, 0).getDate();
		const daysInPreviousMonth = new Date(year, month - 1, 0).getDate();
	
		// Get the first day of the current month (0 = Sunday, 1 = Monday, etc.)
		const firstDayOfWeek = new Date(year, month - 1, 1).getDay();
	
		// Array to store the calendar days (each row represents a week)
		const calendar: IDay[][] = [];
		let week: IDay[] = [];
	
		// Add days from the previous month if the first day of the current month isn't Sunday
		for (let i = firstDayOfWeek - 1; i >= 0; i--) {
			const prevMonth = month === 1 ? 12 : month - 1;
			const prevYear = month === 1 ? year - 1 : year;

			const day: IDay = {
				date: daysInPreviousMonth - i,
				month: prevMonth,
				year: prevYear,
				isCurrentMonth: false,
				state: this.getDayClass({ date: i, isCurrentMonth: false } as IDay),
			};
			week.push(day);
		}
	
		// Add days for the current month
		for (let i = 1; i <= daysInCurrentMonth; i++) {
			const day: IDay = {
				date: i,
				month: month,
				year: year,
				isCurrentMonth: true,
				state: this.getDayClass({ date: i, isCurrentMonth: true } as IDay),
				status: this.getReservationStatus(i, month, year),
			};
			week.push(day);
			if (week.length === 7) {
				calendar.push(week);
				week = [];
			}
		}
	
		// Add days from the next month to fill the last week if necessary
		for (let i = 1; week.length > 0 && week.length < 7; i++) {		
			const nextMonth = month === 12 ? 1 : month + 1;
			const nextYear = month === 12 ? year + 1 : year;

			const day: IDay = {
				date: i,
				month: nextMonth,
				year: nextYear,
				isCurrentMonth: false,
				state: this.getDayClass({ date: i, isCurrentMonth: false } as IDay),
			};
			week.push(day);
		}
		if (week.length > 0) {
			calendar.push(week);
		}
	
		this.weeksOfMonth = calendar;
		this.monthToDisplay = this.months[this.calendarViewModel!.displayDate.month - 1];
	}

	getReservationStatus(day: number, month: number, year: number): 'Confirmed' | 'Pending' | 'Unconfirmed' | 'Cancelled' | 'Legacy' | undefined {
		if (!this.calendarViewModel) {
			return;
		}
		const currentUserReservation = this.calendarViewModel.currentUserReservations.find(x => x.day === day && x.month === month && x.year === year);
		const reservationStatus = currentUserReservation?.status;
		return reservationStatus;
	}

	isDayReserved(year: number, month: number, day: number): boolean {
		if (!this.calendarViewModel) {
			return false;
		}
		return this.calendarViewModel.occupiedDates.some((reservationDate) => reservationDate.day === day && reservationDate.month === month && reservationDate.year === year);
	}

	getReservationType(year: number, month: number, day: number): string {
		if (!this.calendarViewModel) {
			return '';
		}
		const reservationType = this.calendarViewModel.currentUserReservations.find(x => x.day === day && x.month === month && x.year === year)?.type;
		return reservationType ?? 'Substitution';
	}

	getDayClass(day: IDay): string {
		if (!this.calendarViewModel) {
			return '';
		}

    // Colour 4 - Disabled (if the day is not part of the current month)
    if (!day.isCurrentMonth) {
			return 'Disabled';
		}

    // Colour 5 - Greyish (if the day has already passed and it's in the current month)
    if (this.isPast(day)) {
			return 'Greyish';
    }
		
		// Check if current user has a reservation for this day
		const userReservation = this.calendarViewModel.currentUserReservations.find(
			x => x.day === day.date && x.month === this.calendarViewModel!.displayDate.month && x.year === this.calendarViewModel!.displayDate.year
		);

		// Check if there's any reservation for this day on this boat
		const dayIsReserved = this.isDayReserved(this.calendarViewModel.displayDate.year, this.calendarViewModel.displayDate.month, day.date);

		if (userReservation) {
			// User has a reservation - show their reservation type (Standard/Substitution/Contingency)
			return userReservation.type;
		} else if (dayIsReserved) {
			// Day is reserved by someone else - show as Substitution (red) to indicate it's occupied
			return 'Substitution';
		}

    // Day is free - determine available reservation type based on current time and date
    if (this.isToday(day)) {
			// Colour 2 - Contingency (if it's today and after 07:00) or Standard (if it's today and before 07:00)
			if (this.calendarViewModel.currentDate.hour > 6) {
				return 'Contingency';
			}
			return 'Standard';
    } else {
			// Colour 3 - Standard (if the day is free and in the current month)
			return 'Standard';
    }
	}

	// Helper method to check if a day is today using Luxon
	isToday(day: IDay): boolean {
		if (!this.calendarViewModel) {
			return false;
		}

		const selectedDate = DateTime.local(this.calendarViewModel.displayDate.year, this.calendarViewModel.displayDate.month, day.date);
		return this.calendarViewModel.currentDate.hasSame(selectedDate, 'day');
	}

	// Helper method to check if a day has passed in the current month
	isPast(day: IDay): boolean {
		if (!this.calendarViewModel) {
			return false;
		}
		const currentDate = this.calendarViewModel.currentDate;
		const comparisonDate = DateTime.local(this.calendarViewModel.displayDate.year, this.calendarViewModel.displayDate.month, day.date);
		
    if (currentDate.hasSame(comparisonDate, 'day')) {
			return false;
    }
		return comparisonDate < currentDate && day.isCurrentMonth;
	}

}



/*
	get all the the reservations by boat id
	let's say we have reservations on october: 5th, 12th, 17th and 26th

	let's open up each reservation for testing purposes:
	- 5th {
		userId: 'Nick'
		status: 'Unconfirmed'
		type: 'Standard'
	}

	- 4th {
		userId: 'Sophie'
		status: 'Confirmed'
		type: 'Standard'
	}

	- 17th {
		userId: 'Sophie'
		status: 'Pending'
		type: 'Standard'
	}

	- 26th {
		userId: 'Nick'
		status: 'Pending'
		type: 'Standard'
	}

	if I log in as Sophie, I'll see my 2 reservations on the 4th and 17th + 2 reservations from Nick on the 5th and 26th:
	4th - Standard Blue with a confirmed svg
	17th - Standard Blue with a pending svg

	5th - Substitution Red.
	26th - Substitution Red.

	if I log in as Nick, I'll see my 2 reservations on the 5th and 26th + 2 reservations from Sophie on the 4th and 17th:
	5th - Standard Blue with an unconfirmed svg
	26th - Standard Blue with a pending svg

	4th - Substitution Red.
	17th - Substitution Red.

	if I then make a reservation as Sophie on the 5th, I'll see my 3 reservations on the 4th, 5th and 17th + 1 reservation from Nick on the 26th:
	4th - Standard Blue with a confirmed svg
	5th - Substitution Red with a pending svg. Because Nick hasn't cancelled.
	17th - Standard Blue with a pending svg
	26th - Substitution Red.




*/