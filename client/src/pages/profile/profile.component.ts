import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IUser } from '../../models/user';
import { UserService } from '../../services/user.service';
import { BoatService } from '../../services/boat.service';
import { IBoat } from '../../models/boat';
import { IReservation } from '../../models/reservation';
import { IChartBar, IChartSlice } from '../../models/chart';
import { ReservationService } from '../../services/reservation.service';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../auth/auth.service';
import { UiReservationDescriptionComponent } from '../../components/ui-reservation-description/ui-reservation-description.component';
import { UiDonutChartComponent } from '../../components/charts/ui-donut-chart/ui-donut-chart.component';
import { UiBarChartComponent } from '../../components/charts/ui-bar-chart/ui-bar-chart.component';

const MONTHS_SHORT = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'];
const MONTHS_LONG = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    UiReservationDescriptionComponent,
    UiDonutChartComponent,
    UiBarChartComponent,
  ],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  user: IUser | null = null;
  boat: IBoat | null = null;
  reservations: IReservation[] = [];

  isEditing = false;
  isSaving = false;
  newPasswordForm: FormGroup;

  quotaSlices: IChartSlice[] = [];
  monthlyBars: IChartBar[] = [];

  constructor(
    private _userService: UserService,
    private _boatService: BoatService,
    private _authService: AuthService,
    private _reservationService: ReservationService,
    private fb: FormBuilder,
  ) {
    this.newPasswordForm = this.fb.group({
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  ngOnInit(): void {
    this._userService.currentUser$.subscribe(user => {
      if (!user) {
        return;
      }
      this.user = user;
      this.buildQuotaSlices();

      this._boatService.getBoatByBoatId(user.boatId).then(boat => {
        this.boat = boat;
      });

      this._reservationService.getReservationsByUserId(user.userId).then(reservations => {
        this.reservations = [...reservations].sort(this.byDate);
        this.buildMonthlyBars();
      });
    });
  }

  // --- Presentation --------------------------------------------------------

  get initials(): string {
    return (this.user?.name ?? '')
      .split(' ')
      .filter(part => part.length > 0)
      .slice(0, 2)
      .map(part => part[0].toUpperCase())
      .join('');
  }

  get roleLabel(): string {
    return this.user?.role === 'Admin' ? 'Administrador' : 'Membro';
  }

  get totalQuotas(): number {
    if (!this.user) return 0;
    return (this.user.standardQuota ?? 0) + (this.user.substitutionQuota ?? 0) + (this.user.contingencyQuota ?? 0);
  }

  get currentYear(): number {
    return new Date().getFullYear();
  }

  get upcomingReservations(): IReservation[] {
    const today = this.startOfToday();
    return this.reservations.filter(reservation => this.toDate(reservation) >= today);
  }

  formatDate(reservation: IReservation): string {
    return `${reservation.day} de ${MONTHS_LONG[reservation.month - 1]}`;
  }

  weekday(reservation: IReservation): string {
    return this.toDate(reservation).toLocaleDateString('pt-BR', { weekday: 'long' });
  }

  statusTone(status: IReservation['status']): string {
    switch (status) {
      case 'Confirmed': return 'good';
      case 'Pending': return 'warning';
      case 'Unconfirmed': return 'critical';
      default: return 'neutral';
    }
  }

  statusIcon(status: IReservation['status']): string {
    switch (status) {
      case 'Confirmed': return 'bi-check-circle';
      case 'Pending': return 'bi-hourglass-split';
      case 'Unconfirmed': return 'bi-exclamation-circle';
      default: return 'bi-circle';
    }
  }

  statusLabel(status: IReservation['status']): string {
    switch (status) {
      case 'Confirmed': return 'Confirmada';
      case 'Pending': return 'Pendente';
      case 'Unconfirmed': return 'A confirmar';
      case 'Cancelled': return 'Cancelada';
      case 'Legacy': return 'Arquivada';
      default: return '—';
    }
  }

  typeLabel(type: IReservation['type']): string {
    switch (type) {
      case 'Standard': return 'Padrão';
      case 'Substitution': return 'Suplência';
      case 'Contingency': return 'Contingência';
      default: return type;
    }
  }

  typeKey(type: IReservation['type']): string {
    return type.toLowerCase();
  }

  // --- Actions -------------------------------------------------------------

  changeEditState(): void {
    this.isEditing = !this.isEditing;
    this.newPasswordForm.reset();
  }

  confirmChanges(): void {
    if (!this.user || this.newPasswordForm.invalid) {
      this.newPasswordForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this._authService.updateUserPassword(this.user.userId, this.newPasswordForm.value.password)
      .then(() => {
        this.changeEditState();
      })
      .catch(error => {
        console.error('Error updating password:', error);
      })
      .finally(() => {
        this.isSaving = false;
      });
  }

  // --- Internals -----------------------------------------------------------

  private buildQuotaSlices(): void {
    this.quotaSlices = this.user ? [
      { label: 'Padrão', value: this.user.standardQuota ?? 0, color: 'var(--series-standard)' },
      { label: 'Suplência', value: this.user.substitutionQuota ?? 0, color: 'var(--series-substitution)' },
      { label: 'Contingência', value: this.user.contingencyQuota ?? 0, color: 'var(--series-contingency)' },
    ] : [];
  }

  private buildMonthlyBars(): void {
    const year = this.currentYear;
    const perMonth = new Array(12).fill(0);

    this.reservations
      .filter(reservation => reservation.year === year)
      .forEach(reservation => {
        perMonth[reservation.month - 1] += 1;
      });

    this.monthlyBars = perMonth.map((value, index) => ({
      label: MONTHS_SHORT[index],
      fullLabel: `${MONTHS_LONG[index]} de ${year}`,
      value
    }));
  }

  private toDate(reservation: IReservation): Date {
    return new Date(reservation.year, reservation.month - 1, reservation.day);
  }

  private startOfToday(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private byDate = (a: IReservation, b: IReservation): number =>
    this.toDate(a).getTime() - this.toDate(b).getTime();
}
