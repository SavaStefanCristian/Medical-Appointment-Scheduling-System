import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService, RegisterDto } from '../../services/auth';

import { RouterModule, Router } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class Register {
  private authService = inject(AuthService);
  private router = inject(Router);

  name = '';
  phone = '';
  email = '';
  password = '';

  errorMessage = '';
  register() {
    const dto: RegisterDto = { name: this.name, phone: this.phone, email: this.email, password: this.password };
    this.authService.register(dto).subscribe({
      next: res => {
        localStorage.setItem('token', res.token);
        this.router.navigate(['/home']);
      },
      error: () => {
        this.errorMessage = 'Parola sau email gresit!';
      }
    });
  }
}
