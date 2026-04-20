import { Injectable } from '@angular/core';
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

  private baseUrl = `${environment.apiUrl}/Doctors`;

  constructor(private http: HttpClient) {}

  getDoctors(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.baseUrl);
  }
}
