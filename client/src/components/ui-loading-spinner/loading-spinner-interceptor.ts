import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable, finalize } from 'rxjs';
import { Injectable, signal } from '@angular/core';
import { LoadingSpinnerService } from './loading-spinner.service';

@Injectable()
export class LoadingSpinnerInterceptor implements HttpInterceptor {
  #loading = signal(false);
  loading = this.#loading.asReadonly();

  constructor(
    private _loadingSpinnerService: LoadingSpinnerService,
  ) {
  }

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    this._loadingSpinnerService.startLoading();
    return next.handle(request).pipe(
      finalize(() => {
        this._loadingSpinnerService.stopLoading();
      })
    );
  }
}
