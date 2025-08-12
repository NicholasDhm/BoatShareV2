import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { CommonModule } from '@angular/common';
import { IReservation } from '../../models/reservation';
import { IUser } from '../../models/user';
import { ConfirmationModalComponent } from './confirmation-modal/confirmation-modal.component';
import { StandardModalComponent } from './standard-modal/standard-modal.component';

@Component({
  selector: 'app-reservation-modal',
  standalone: true,
  imports: [
    UiCardComponent,
    CommonModule,
    ConfirmationModalComponent,
    StandardModalComponent
  ],
  templateUrl: './reservation-modal.component.html',
  styleUrls: ['./reservation-modal.component.scss']
})
export class ReservationModalComponent implements OnInit, OnChanges {
  @Input() userReservationToDisplay: IReservation | null = null;
  @Input() firstReservationToDisplay: IReservation | null = null;

  @Input() activeUser: IUser | null = null;
  @Input() firstReservationUser: IUser | null = null;
  
  @Output() close = new EventEmitter<void>();
  @Output() newReservation = new EventEmitter<IReservation | null>();
  @Output() cancelReservation = new EventEmitter<IReservation | null>();
  @Output() confirmReservation = new EventEmitter<IReservation | null>();
  weekday = '';
  reservationDate: Date | null = null;
  confirmationDate: Date | null = null;

  ngOnInit(): void {
    if (!this.userReservationToDisplay) {
      return;
    }

    // Get the weekday and the reservation date
    this.weekday = new Date(this.userReservationToDisplay.year, this.userReservationToDisplay.month - 1, this.userReservationToDisplay.day)
      .toLocaleDateString('pt-BR', { weekday: 'long' });
    this.reservationDate = new Date(this.userReservationToDisplay.year, this.userReservationToDisplay.month - 1, this.userReservationToDisplay.day);
    this.confirmationDate = new Date(this.userReservationToDisplay.year, this.userReservationToDisplay.month - 1, this.userReservationToDisplay.day - 3);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['userReservationToDisplay']) {
      this.ngOnInit();
    }
  }

  onConfirmReservation(): void {
    this.confirmReservation.emit(this.userReservationToDisplay);
    this.close.emit();
  }

  onCancelReservation(): void {
    this.cancelReservation.emit(this.userReservationToDisplay);
    this.close.emit();
  }

  makeReservation() {
    this.newReservation.emit(this.userReservationToDisplay);
    this.close.emit();
  }

  closeModal(): void {
    this.close.emit();
  }
}
