import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';
import { ReservationService } from '../../services/reservation.service';
import { IReservation } from '../../models/reservation';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-manage-boats',
  standalone: true,
  imports: [
    UiCardComponent,
    CommonModule,
    FormsModule,
  ],
  templateUrl: './manage-boats.component.html',
  styleUrls: ['./manage-boats.component.scss']
})
export class ManageBoatsComponent implements OnInit {
  boats: IBoat[] = [];
  reservationsByBoatId: IReservation[] = [];
  boat: IBoat | null = null;
  boatId: number | null = null;

  addBoatModal = false;
  newBoatName: string = '';
  newBoatCapacity: number = 0;
  deleteBoatId: number = 0;

  constructor(
    private _boatService: BoatService,
    private _reservationService: ReservationService,
  ) {
    this._boatService.getAllBoats().then(boats => {
      this.boats = boats;
      if (boats?.length > 0) {
        const firstBoat = boats[0];
        this.onSelectBoatId(firstBoat.boatId);
      }
    });
  }

  ngOnInit(): void {
    this._boatService.boats$.subscribe(boats => {
      this.boats = boats;
    });
  }

  onSelectBoatId(boatId: number | null): void {
    if (boatId) {
      this.boatId = boatId;
      this.boat = this.boats?.find(x => x.boatId === boatId) || null;
      this.getReservationsByBoatId(boatId);
    } else {
      this.boat = null;
    }
  }

  private getReservationsByBoatId(boatId: number) {
    this._reservationService.getReservationsByBoatId(boatId).then(reservationsByBoatId => {
      reservationsByBoatId.forEach(reservation => {
        if (!this.reservationsByBoatId.some(r => r.reservationId === reservation.reservationId)) {
          this.reservationsByBoatId.push(reservation);
        }
      });
    });
  }

  changeBoatModalState(): void {
    this.addBoatModal = !this.addBoatModal;
  }

  addNewBoat(): void {
    const newBoat: IBoat = {
      boatId: 0,
      name: this.newBoatName || '',
      capacity: this.newBoatCapacity || 0,
      assignedUsersCount: 0,
    };

    this._boatService.postBoat(newBoat).then(boat => {
      this.boats.push(boat);
      this.onSelectBoatId(boat.boatId);
      this.addBoatModal = false;
    });
  }

  deleteBoat(): void {
    this._boatService.deleteBoatById(this.deleteBoatId).then(() => {
      const newBoats = this.boats.filter(x => x.boatId !== this.deleteBoatId);
      this.boats = Object.assign({}, newBoats);
      this.onSelectBoatId(this.boats[0]?.boatId);
    });
  }
}