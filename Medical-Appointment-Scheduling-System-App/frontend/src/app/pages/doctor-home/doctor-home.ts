import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-doctor-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './doctor-home.html',
  styleUrls: ['./doctor-home.css']
})
export class DoctorHome {

  constructor(private router: Router) {}

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('patientId');
    this.router.navigate(['/login']);
  }
}
