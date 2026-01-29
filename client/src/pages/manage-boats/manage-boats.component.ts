import { Component, OnInit } from '@angular/core';
import { UiCardComponent } from '../../components/ui-card/ui-card.component';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';
import { ReservationService } from '../../services/reservation.service';
import { IReservation } from '../../models/reservation';
import { UserService } from '../../services/user.service';
import { IUser } from '../../models/user';
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
  currentUser: IUser | null = null;

  addBoatModal = false;
  newBoatName: string = '';
  newBoatCapacity: number = 0;
  newBoatImageUrl: string = '';
  deleteBoatId: number = 0;

  isEditingImage = false;
  editImageUrl: string = '';

  constructor(
    private _boatService: BoatService,
    private _reservationService: ReservationService,
    private _userService: UserService,
  ) {
    this._userService.currentUser$.subscribe(user => {
      this.currentUser = user;
    });
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
      imageUrl: this.newBoatImageUrl || undefined,
    };

    this._boatService.postBoat(newBoat).then(boat => {
      this.boats.push(boat);
      this.onSelectBoatId(boat.boatId);
      this.addBoatModal = false;
      this.newBoatName = '';
      this.newBoatCapacity = 0;
      this.newBoatImageUrl = '';
    });
  }

  deleteBoat(): void {
    this._boatService.deleteBoatById(this.deleteBoatId).then(() => {
      const newBoats = this.boats.filter(x => x.boatId !== this.deleteBoatId);
      this.boats = Object.assign({}, newBoats);
      this.onSelectBoatId(this.boats[0]?.boatId);
    });
  }

  getBoatReservations(): IReservation[] {
    if (!this.boat) return [];
    return this.reservationsByBoatId.filter(r => r.boatId === this.boat?.boatId);
  }

  formatReservationDate(day: number, month: number, year: number): string {
    const monthNames = [
      'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    return `${day} de ${monthNames[month - 1]} de ${year}`;
  }

  get canEditBoat(): boolean {
    return this.currentUser !== null &&
           this.boat !== null &&
           this.currentUser.boatId === this.boat.boatId;
  }

  toggleEditImage(): void {
    if (!this.isEditingImage && this.boat) {
      this.editImageUrl = this.boat.imageUrl || '';
    }
    this.isEditingImage = !this.isEditingImage;
  }

  saveImageUrl(): void {
    if (!this.boat) return;

    this._boatService.updateBoat(this.boat.boatId, { imageUrl: this.editImageUrl || undefined }).then(updatedBoat => {
      this.boat = updatedBoat;
      const index = this.boats.findIndex(b => b.boatId === updatedBoat.boatId);
      if (index !== -1) {
        this.boats[index] = updatedBoat;
      }
      this.isEditingImage = false;
    });
  }
}