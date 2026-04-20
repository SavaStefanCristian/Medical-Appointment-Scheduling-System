import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Doctor, DoctorService } from '../../services/doctor';
import { AppointmentService } from '../../services/appointment';

import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {
  private doctorService = inject(DoctorService);
  private router = inject(Router);
  private appointmentService = inject(AppointmentService);

  doctors: Doctor[] = [];
  filteredDoctors: Doctor[] = [];
  specialties: string[] = [];
  selectedSpecialty = '';
  searchName = '';

  selectedDoctor: Doctor | null = null;
  appointmentDate = '';
  errorMessage = '';
  successMessage = '';

  ngOnInit() {
    this.loadDoctors();
  }

  loadDoctors() {
    this.doctorService.getDoctors().subscribe({
      next: (data) => {
        this.doctors = data;
        this.filteredDoctors = data;
        this.specialties = Array.from(new Set(data.map(d => d.specialty)));
      },
      error: (err) => console.error('Eroare la preluarea doctorilor', err)
    });
  }

  filterBySpecialty() {
    this.filteredDoctors = this.doctors.filter(d =>
      (!this.selectedSpecialty || d.specialty === this.selectedSpecialty) &&
      (!this.searchName || d.name.toLowerCase().includes(this.searchName.toLowerCase()))
    );
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }

  goToAppointments() {
    this.router.navigate(['/appointments']);
  }

  bookAppointment(doctor: Doctor) {
    this.selectedDoctor = doctor;
    this.errorMessage = '';
    this.successMessage = '';
  }

  confirmAppointment() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.appointmentDate) {
      this.errorMessage = 'Selectează o dată.';
      return;
    }

    const selectedDate = new Date(this.appointmentDate);
    if (selectedDate < new Date()) {
      this.errorMessage = 'Nu poți selecta o dată din trecut.';
      return;
    }

    const patientId = Number(localStorage.getItem('patientId'));

    const dto = {
      doctorId: this.selectedDoctor!.id,
      patientId: patientId,
      appointmentDate: this.appointmentDate
    };

    this.appointmentService.createAppointment(dto).subscribe({
      next: () => {
        this.successMessage = 'Programare creată cu succes!';
        this.selectedDoctor = null;
      },
      error: (err) => {
        this.errorMessage = err.error || 'Eroare la programare.';
      }
    });
  }
}
