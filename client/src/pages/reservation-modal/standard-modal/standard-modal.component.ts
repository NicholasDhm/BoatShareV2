import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { UiCardComponent } from '../../../components/ui-card/ui-card.component';
import { CommonModule, registerLocaleData } from '@angular/common';
import { IUser } from '../../../models/user';
import { LOCALE_ID } from '@angular/core';
import localePt from '@angular/common/locales/pt';

@Component({
  selector: 'app-standard-modal',
  standalone: true,
  imports: [UiCardComponent, CommonModule],
  providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }],
  templateUrl: './standard-modal.component.html',
  styleUrls: ['./standard-modal.component.scss']
})
export class StandardModalComponent implements OnInit {
  @Input() weekday: string = '';
  @Input() confirmationDate: Date | null = null;
  @Input() reservationDate: Date | null = null;
  @Input() reservationType: string = '';
  @Input() status: string = '';

  @Input() firstReservationUserName: string | null = null;
  @Input() activeUser: IUser | null = null;
  
  @Output() close = new EventEmitter<void>();
  @Output() reserve = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  reservationTypeInPt: string = '';

  constructor() {
    registerLocaleData(localePt);
  }

  ngOnInit(): void {
    if (this.reservationType) {
      if (this.reservationType === 'Standard') {
        this.reservationTypeInPt = 'Padrão';
      } else if (this.reservationType === 'Substitution') {
        this.reservationTypeInPt = 'Suplência';
      } else {
        this.reservationTypeInPt = 'Contingência';
      }    
    }
  }

  makeReservation() {
    this.reserve.emit();
  }

  onCancelReservation() {
    this.cancel.emit();
  }

  closeModal() {
    this.close.emit();
  }
}