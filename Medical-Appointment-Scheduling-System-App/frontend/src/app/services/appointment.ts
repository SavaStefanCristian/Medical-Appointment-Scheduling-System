import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CreateAppointmentDto {
  doctorId: number;
  patientId: number;
  appointmentDate: string;
}

@Injectable({ providedIn: 'root' })
export class AppointmentService {

  private baseUrl = 'http://localhost:8080/api/Appointments';

  constructor(private http: HttpClient) {}

  createAppointment(dto: CreateAppointmentDto): Observable<any> {
    return this.http.post(this.baseUrl, dto);
  }

  getDoctorAppointments(doctorId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/doctor/${doctorId}`);
  }
}
