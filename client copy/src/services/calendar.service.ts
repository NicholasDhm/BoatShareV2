import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { ICalendarViewModel } from '../models/calendar';
import { ReservationService } from './reservation.service';
import { DateTime } from 'luxon';
import { UserService } from './user.service';
import { IUser } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  user: IUser | null = null;
	
  private _calendarViewModelSubject = new BehaviorSubject<ICalendarViewModel | null>(null);
  calendarViewModel$ = this._calendarViewModelSubject.asObservable();

  constructor(
    private _reservationService: ReservationService,
    private _userService: UserService,
  ) {}

  updateCalendarViewModel(calendarViewModel: ICalendarViewModel): void {
    this._calendarViewModelSubject.next(calendarViewModel);
  }

  getCalendarViewModel(): ICalendarViewModel | null {
    return this._calendarViewModelSubject.value;
  }

  loadCalendarViewModel(displayDate?: DateTime): void {
    const user = this._userService.getCurrentUser();
    if (!user) {
      return;
    }

    const currentDate = DateTime.local();
    const boatId = user.boatId ?? '';
    
    Promise.all([
      this._reservationService.getReservationsByBoatId(boatId),
      this._reservationService.getReservationsByUserId(user.userId)
    ]).then(([occupiedDates, userReservations]) => {
      const viewModel: ICalendarViewModel = {
        currentDate: currentDate,
        displayDate: displayDate ?? currentDate,
        occupiedDates: occupiedDates,
        currentUserReservations: userReservations || [],
      };
      this.updateCalendarViewModel(viewModel);
    }).catch(error => {
      console.error("Error loading calendar data:", error);
    });
  }

  updateCalendarMonth(month: number): void {
    let updatedViewModel = this.getCalendarViewModel();
    if (updatedViewModel) {
      updatedViewModel = Object.assign({}, updatedViewModel, { displayDate: updatedViewModel.displayDate.plus({ months: month }) });
      this.updateCalendarViewModel(updatedViewModel);
    }
  }
}