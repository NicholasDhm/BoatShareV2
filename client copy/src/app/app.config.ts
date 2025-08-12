import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi } from '@angular/common/http';
import { authInterceptor } from '../auth/auth.interceptor';
import { LoadingSpinnerInterceptor } from '../components/ui-loading-spinner/loading-spinner-interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }), 
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideHttpClient(withInterceptors([authInterceptor])),
    { provide: HTTP_INTERCEPTORS, useClass: LoadingSpinnerInterceptor, multi: true },
  ]
};
