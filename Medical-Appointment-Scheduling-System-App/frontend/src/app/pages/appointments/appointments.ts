import { Component, OnInit } from '@angular/core';
import { Appointment, AppointmentService } from '../../services/appointment';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-appointments',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './appointments.html',
  styleUrls: ['./appointments.css']
})
export class Appointments implements OnInit {
  appointments: Appointment[] = [];
  errorMessage = '';

  constructor(private service: AppointmentService) {}

  ngOnInit() {
    this.load();
  }

  load() {
    this.service.getMyAppointments().subscribe({
      next: res => this.appointments = res,
      error: err => {
        this.errorMessage = 'Eroare la încărcare';
        console.error(err);
      }
    });
  }

  cancel(id: number) {
    this.service.cancelAppointment(id).subscribe({
      next: () => {
        this.load(); // 🔄 refresh
      },
      error: err => {
        alert('Nu s-a putut anula programarea');
        console.error(err);
      }
    });
  }
}
