import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Doctor, DoctorService } from '../../services/doctor';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class Home implements OnInit {
  doctors: Doctor[] = [];
  filteredDoctors: Doctor[] = [];
  specialties: string[] = [];
  selectedSpecialty = '';
  searchName = '';

  constructor(private doctorService: DoctorService, private router: Router) {}

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

  bookAppointment(doctor: Doctor) {
    alert(`Programare pentru Dr. ${doctor.name} (UI only)`);
  }
}
