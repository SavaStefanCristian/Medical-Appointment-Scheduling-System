import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {environment} from '../../environments/environment';

export interface CreateAppointmentDto {
  doctorId: number;
  patientId: number;
  appointmentDate: string;
}

export interface Appointment {
  id: number;
  doctorId: number;
  patientId: number;
  appointmentDate: string;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private http = inject(HttpClient);


  private baseUrl = `${environment.apiUrl}/Appointments`;

  createAppointment(dto: CreateAppointmentDto): Observable<any> {
    return this.http.post(this.baseUrl, dto);
  }

  getDoctorAppointments(doctorId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/doctor/${doctorId}`);
  }

  getMyAppointments(): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.baseUrl}/my`);
  }

  cancelAppointment(id: number): Observable<any> {
    return this.http.patch(`${this.baseUrl}/${id}/cancel`, {});
  }

  updateStatus(id: number, status: string): Observable<any> {
    return this.http.patch(`${this.baseUrl}/${id}/status`, { status });
  }
}
