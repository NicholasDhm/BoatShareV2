import { Injectable } from '@angular/core';
import { IUser } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class ValidationService {

  validateQuota(user: IUser, reservationType: string): string | null {
    switch (reservationType) {
      case 'Standard':
        return user.standardQuota > 0 ? null : 'Cotas padrões insuficientes';
      case 'Substitution':
        return user.substitutionQuota > 0 ? null : 'Cotas suplentes insuficientes';
      case 'Contingency':
        return user.contingencyQuota > 0 ? null : 'Cotas contingentes insuficientes';
      default:
        return 'Invalid reservation type';
    }
  }

  // Add more validations as needed
}
