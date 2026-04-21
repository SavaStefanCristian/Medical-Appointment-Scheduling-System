import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from '../../environments/environment';

export interface Doctor {
  id: number;
  name: string;
  specialty: string;
  userId: number;
}

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private http = inject(HttpClient);


  private baseUrl = `${environment.apiUrl}/Doctors`;

  getDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.baseUrl);
  }
}
