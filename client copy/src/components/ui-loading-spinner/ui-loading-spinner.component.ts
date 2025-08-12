import { Component, Signal } from '@angular/core';
import { LoadingSpinnerService } from './loading-spinner.service';

@Component({
  selector: 'ui-loading-spinner',
  standalone: true,
  imports: [],
  templateUrl: './ui-loading-spinner.component.html',
  styleUrl: './ui-loading-spinner.component.scss'
})
export class UiLoadingSpinnerComponent {
  loading: Signal<boolean>;

  constructor(
    private _loadingSpinnerService: LoadingSpinnerService,
  ) {
    this.loading = this._loadingSpinnerService.loading;
  }
}