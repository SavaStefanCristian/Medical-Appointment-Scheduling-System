import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Home } from './pages/home/home';
import { authGuard } from './services/auth.guard';
import { Appointments } from './pages/appointments/appointments';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'home', component: Home,canActivate: [authGuard] },
  { path: 'appointments', component: Appointments },
];
