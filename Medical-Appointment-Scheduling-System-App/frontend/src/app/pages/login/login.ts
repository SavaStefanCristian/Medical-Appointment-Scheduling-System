import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule, RouterModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class Login {
  email = '';
  password = '';
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  login() {
    this.errorMessage = '';

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
      next: res => {
        localStorage.setItem('token', res.token);

        if (res.patientId) {
          localStorage.setItem('patientId', res.patientId.toString());
        }

        // 👉 optional (debug)
        console.log('Login response:', res);

        this.router.navigate(['/home']);
      },
      error: err => {
        this.errorMessage = 'Parola sau email gresit!';
        console.error(err);
      }
    });
  }
}
