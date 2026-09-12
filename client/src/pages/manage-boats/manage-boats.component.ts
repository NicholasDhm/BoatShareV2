import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';
import { ReservationService } from '../../services/reservation.service';
import { IReservation } from '../../models/reservation';
import { UserService } from '../../services/user.service';
import { IUser } from '../../models/user';
import { IChartBar } from '../../models/chart';
import { UiStatTileComponent } from '../../components/charts/ui-stat-tile/ui-stat-tile.component';
import { UiMeterComponent } from '../../components/charts/ui-meter/ui-meter.component';
import { UiBarChartComponent } from '../../components/charts/ui-bar-chart/ui-bar-chart.component';

const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

@Component({
  selector: 'app-manage-boats',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    UiStatTileComponent,
    UiMeterComponent,
    UiBarChartComponent,
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
  newBoatName = '';
  newBoatCapacity = 0;
  newBoatImageUrl = '';

  deleteBoatId: number | null = null;
  /** Two-step delete: the id is only removed once the same button is pressed again. */
  pendingDeleteId: number | null = null;

  isEditingImage = false;
  editImageUrl = '';

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
        this.onSelectBoatId(boats[0].boatId);
      }
    });
  }

  ngOnInit(): void {
    this._boatService.boats$.subscribe(boats => {
      this.boats = boats;
    });
  }

  // --- Selection -----------------------------------------------------------

  onSelectBoatId(boatId: number | null): void {
    if (!boatId) {
      this.boat = null;
      return;
    }
    this.boatId = boatId;
    this.boat = this.boats?.find(x => x.boatId === boatId) || null;
    this.getReservationsByBoatId(boatId);
  }

  // --- Insights ------------------------------------------------------------

  get totalCapacity(): number {
    return this.boats.reduce((total, boat) => total + boat.capacity, 0);
  }

  get totalAssigned(): number {
    return this.boats.reduce((total, boat) => total + boat.assignedUsersCount, 0);
  }

  get openSeats(): number {
    return Math.max(0, this.totalCapacity - this.totalAssigned);
  }

  get occupancyRate(): number {
    if (this.totalCapacity === 0) return 0;
    return Math.round((this.totalAssigned / this.totalCapacity) * 100);
  }

  /** Cotistas por embarcação — uma série. */
  get seatsPerBoatBars(): IChartBar[] {
    return this.boats.map(boat => ({
      label: boat.name.length > 10 ? `${boat.name.slice(0, 9)}…` : boat.name,
      fullLabel: `${boat.name} — ${boat.assignedUsersCount} de ${boat.capacity} vagas`,
      value: boat.assignedUsersCount
    }));
  }

  get boatMonthlyBars(): IChartBar[] {
    const year = new Date().getFullYear();
    const perMonth = new Array(12).fill(0);

    this.getBoatReservations()
      .filter(reservation => reservation.year === year)
      .forEach(reservation => {
        perMonth[reservation.month - 1] += 1;
      });

    return perMonth.map((value, index) => ({
      label: MONTHS_LONG[index].slice(0, 3),
      fullLabel: `${MONTHS_LONG[index]} de ${year}`,
      value
    }));
  }

  get currentYear(): number {
    return new Date().getFullYear();
  }

  // --- Actions -------------------------------------------------------------

  changeBoatModalState(): void {
    this.addBoatModal = !this.addBoatModal;
    this.pendingDeleteId = null;
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
      this.boats = [...this.boats, boat];
      this.onSelectBoatId(boat.boatId);
      this.addBoatModal = false;
      this.newBoatName = '';
      this.newBoatCapacity = 0;
      this.newBoatImageUrl = '';
    });
  }

  requestDelete(): void {
    if (!this.deleteBoatId) {
      return;
    }
    this.pendingDeleteId = this.deleteBoatId;
  }

  cancelDelete(): void {
    this.pendingDeleteId = null;
  }

  confirmDelete(): void {
    const boatId = this.pendingDeleteId;
    if (!boatId) {
      return;
    }

    this._boatService.deleteBoatById(boatId).then(() => {
      this.boats = this.boats.filter(x => x.boatId !== boatId);
      this.pendingDeleteId = null;
      this.deleteBoatId = null;
      this.onSelectBoatId(this.boats[0]?.boatId ?? null);
    });
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

  // --- Helpers -------------------------------------------------------------

  getBoatReservations(): IReservation[] {
    if (!this.boat) return [];
    return this.reservationsByBoatId
      .filter(reservation => reservation.boatId === this.boat?.boatId)
      .sort((a, b) => new Date(a.year, a.month - 1, a.day).getTime() - new Date(b.year, b.month - 1, b.day).getTime());
  }

  formatReservationDate(day: number, month: number, year: number): string {
    return `${day} de ${MONTHS_LONG[month - 1]} de ${year}`;
  }

  typeLabel(type: string): string {
    switch (type) {
      case 'Standard': return 'Padrão';
      case 'Substitution': return 'Suplência';
      case 'Contingency': return 'Contingência';
      default: return type;
    }
  }

  typeKey(type: string): string {
    return type.toLowerCase();
  }

  get canEditBoat(): boolean {
    return this.currentUser !== null &&
           this.boat !== null &&
           this.currentUser.boatId === this.boat.boatId;
  }

  get pendingDeleteName(): string {
    const boat = this.boats.find(b => b.boatId === this.pendingDeleteId);
    return boat ? boat.name : `#${this.pendingDeleteId}`;
  }

  private getReservationsByBoatId(boatId: number): void {
    this._reservationService.getReservationsByBoatId(boatId).then(reservations => {
      reservations.forEach(reservation => {
        if (!this.reservationsByBoatId.some(r => r.reservationId === reservation.reservationId)) {
          this.reservationsByBoatId.push(reservation);
        }
      });
    });
  }
}
