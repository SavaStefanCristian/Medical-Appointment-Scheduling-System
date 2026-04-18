import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AppointmentService } from '../../services/appointment';

@Component({
  selector: 'app-doctor-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './doctor-home.html',
  styleUrls: ['./doctor-home.css']
})
export class DoctorHome implements OnInit {

  appointments: any[] = [];
  doctorId: number = 0;

  constructor(
    private router: Router,
    private appointmentService: AppointmentService
  ) {}

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
      next: data => this.appointments = data,
      error: err => console.error('Eroare la appointments', err)
    });
  }

  approve(id: number) {
    this.appointmentService.updateStatus(id, 'Confirmed').subscribe({
      next: () => this.loadAppointments(),
      error: err => console.error(err)
    });
  }

  complete(id: number) {
    this.appointmentService.updateStatus(id, 'Completed').subscribe({
      next: () => this.loadAppointments(),
      error: err => console.error(err)
    });
  }

  cancel(id: number) {
    this.appointmentService.cancelAppointment(id).subscribe({
      next: () => this.loadAppointments(),
      error: err => console.error(err)
    });
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
