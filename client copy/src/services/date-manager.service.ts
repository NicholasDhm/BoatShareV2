import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { DateTime } from 'luxon';

@Injectable({
  providedIn: 'root'
})
export class DateManagerService {
  private currentDateSubject = new BehaviorSubject<DateTime>(DateTime.now());
  private currentTimeSubject = new BehaviorSubject<number>(DateTime.now().hour);

  currentDate$ = this.currentDateSubject.asObservable();
  currentTime$ = this.currentTimeSubject.asObservable();

  setCurrentDate(date: DateTime): void {
    this.currentDateSubject.next(date);
    this.currentTimeSubject.next(date.hour);
  }

  incrementDate(days: number): void {
    const newDate = this.currentDateSubject.value.plus({ days });
    this.setCurrentDate(newDate);
  }

  decrementDate(days: number): void {
    const newDate = this.currentDateSubject.value.minus({ days });
    this.setCurrentDate(newDate);
  }

  setContingencyTime(hour: number): void {
    const currentDate = this.currentDateSubject.value;
    const newDate = currentDate.set({ hour });
    this.setCurrentDate(newDate);
  }

  resetTime(): void {
    const currentDate = DateTime.now();
    this.setCurrentDate(currentDate);
  }
}
