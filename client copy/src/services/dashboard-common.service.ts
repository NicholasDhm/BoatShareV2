import { Injectable } from '@angular/core';
import { IDay } from '../models/calendar';
import { IReservation, ReservationType } from '../models/reservation';
import { ReservationService } from './reservation.service';
import { IUser } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class DashboardCommonService {

  constructor(
    private _reservationService: ReservationService,
  ) { }



  fetchReservationToDisplay(day: IDay, currentUser: IUser): Promise<IReservation[]> {
    let reservations: IReservation[] = [];
    const reservationType: ReservationType = day.state as ReservationType;
    return this.isDateReserved(day, currentUser).then(isReserved => {
      if (isReserved) {
        return this._reservationService.getReservationByDateAndBoatId(day.date, day.month, day.year, currentUser.boatId).then(firstReservation => {  
          const firstReservationForDate = Object.assign({}, firstReservation);
    
          if (firstReservationForDate.userId === currentUser.userId) {
            reservations.push(firstReservationForDate);
            return Promise.resolve(reservations);
          } else {
            // Otherwise, look for the current user's reservations for the same date
            return this._reservationService.getReservationsByUserId(currentUser.userId).then(currentUserReservations => {
              const selectedReservation = currentUserReservations.find(
                x => x.day === day.date && x.month === day.month && x.year === day.year
              );
    
              if (selectedReservation) {
                const userReservation = Object.assign({}, selectedReservation);
                reservations.push(firstReservationForDate);
                reservations.push(userReservation);
              } else {
                const newReservation: IReservation = {
                  type: reservationType,
                  userId: currentUser.userId,
                  boatId: currentUser.boatId,
                  year: day.year,
                  month: day.month,
                  day: day.date,
                };

                reservations.push(firstReservationForDate);
                reservations.push(newReservation);
              }
    
              return Promise.resolve(reservations);
            });
          }
        });
      } else {
        // If no reservation found, create a new reservation
        const newReservation: IReservation = {
          type: reservationType,
          userId: currentUser.userId,
          boatId: currentUser.boatId,
          year: day.year,
          month: day.month,
          day: day.date,
        };
        reservations.push(newReservation);
        return Promise.resolve(reservations);
      }
    });
  }

  isDateReserved(day: IDay, currentUser: IUser): Promise<boolean> {
    return this._reservationService.getAllReservations().then(reservations => {
      return reservations.some((reservation) => reservation.day === day.date && reservation.month === day.month && reservation.year === day.year && reservation.boatId === currentUser.boatId);
    });
  }
}