import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule, registerLocaleData } from '@angular/common';
import { LOCALE_ID } from '@angular/core';
import localePt from '@angular/common/locales/pt';

@Component({
  selector: 'app-confirmation-modal',
  standalone: true,
  imports: [CommonModule],
  providers: [{ provide: LOCALE_ID, useValue: 'pt-BR' }],
  templateUrl: './confirmation-modal.component.html',
  styleUrls: ['./confirmation-modal.component.scss']
})
export class ConfirmationModalComponent implements OnInit {
  @Input() weekday: string = '';
  @Input() reservationDate: Date | null = null;
  @Input() reservationType: string = '';
  @Input() reserved: boolean = false;
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  reservationTypeInPt: string = '';

  get typeKey(): string {
    return (this.reservationType || 'standard').toLowerCase();
  }

  constructor() {
    registerLocaleData(localePt);
  }

  ngOnInit(): void {
    if (this.reservationType === 'Standard') {
      this.reservationTypeInPt = 'Padrão';
    } else if (this.reservationType === 'Substitution') {
      this.reservationTypeInPt = 'Suplência';
    } else {
      this.reservationTypeInPt = 'Contingência';
    }
  }
  
  onConfirmReservation() {
    this.confirm.emit();
  }

  onCancelReservation() {
    this.cancel.emit();
  }

  closeModal() {
    this.close.emit();
  }
}