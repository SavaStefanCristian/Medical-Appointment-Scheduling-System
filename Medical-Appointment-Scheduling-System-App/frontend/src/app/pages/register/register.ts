import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService, RegisterDto } from '../../services/auth';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  name = '';
  phone = '';
  email = '';
  password = '';

  constructor(private authService: AuthService, private router: Router) {}

  errorMessage = '';
  register() {
    const dto: RegisterDto = { name: this.name, phone: this.phone, email: this.email, password: this.password };
    this.authService.register(dto).subscribe({
      next: res => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: err => {
        this.errorMessage = 'Parola sau email gresit!';
      }
    });
  }
}
