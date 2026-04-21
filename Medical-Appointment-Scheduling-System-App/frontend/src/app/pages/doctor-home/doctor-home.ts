import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { AppointmentService } from '../../services/appointment';
import {Appointment} from '../../services/appointment';

@Component({
  selector: 'app-doctor-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './doctor-home.html',
  styleUrls: ['./doctor-home.css']
})
export class DoctorHome implements OnInit {
  private router = inject(Router);
  private appointmentService = inject(AppointmentService);


  appointments: Appointment[] = [];
  doctorId = 0;

  ngOnInit() {
    const doctorId = localStorage.getItem('doctorId');

    if (!doctorId) {
      this.router.navigate(['/login']);
      return;
    }

    this.doctorId = Number(doctorId);
    this.loadAppointments();
  }

  loadAppointments() {
    this.appointmentService.getDoctorAppointments(this.doctorId).subscribe({
      next: (data: Appointment[]) => this.appointments = data,
      error: (err: HttpErrorResponse) => console.error('Eroare la appointments', err.message)
    });
  }

  approve(id: number) {
    this.appointmentService.updateStatus(id, 'Confirmed').subscribe({
      next: () => this.loadAppointments(),
      error: (err: HttpErrorResponse) => console.error(err)
    });
  }

  complete(id: number) {
    this.appointmentService.updateStatus(id, 'Completed').subscribe({
      next: () => this.loadAppointments(),
      error: (err: HttpErrorResponse) => console.error(err)
    });
  }

  cancel(id: number) {
    this.appointmentService.cancelAppointment(id).subscribe({
      next: () => this.loadAppointments(),
      error: (err: HttpErrorResponse) => console.error(err)
    });
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
