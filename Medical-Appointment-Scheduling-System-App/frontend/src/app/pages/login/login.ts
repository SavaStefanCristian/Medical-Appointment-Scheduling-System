import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styles: [`
    .card { padding: 20px; border: 1px solid #ccc; border-radius: 8px; }
    input { display: block; width: 100%; margin: 8px 0; padding: 8px; }
    button { width: 100%; padding: 10px; margin-top: 10px; }
  `]
})
export class Login {
  email = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  login() {
    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: res => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: err => console.error('Eroare login', err)
    });
  }
}
