import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isRegister = false;
  returnUrl = '/';
  errorMessage = '';
  isLoading = false;

  // Login model
  loginEmail = '';
  loginPassword = '';

  // Register model
  regFullName = '';
  regEmail = '';
  regPassword = '';
  regPhone = '';
  regAddress = '';

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    // If already logged in, redirect
    if (this.authService.isLoggedIn()) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  useDemoAdmin(): void {
    this.useDemoCredentials('admin@swiftbasket.com', 'Admin@123');
  }

  useDemoCustomer(): void {
    this.useDemoCredentials('customer@swiftbasket.com', 'Customer@123');
  }

  private useDemoCredentials(email: string, password: string): void {
    this.isRegister = false;
    this.loginEmail = email;
    this.loginPassword = password;
    this.errorMessage = '';
  }

  onLogin(): void {
    if (!this.loginEmail || !this.loginPassword) {
      this.errorMessage = 'Please enter email and password.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login({
      email: this.loginEmail,
      password: this.loginPassword
    }).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.user.role === 'Admin' && this.returnUrl === '/') {
          this.router.navigate(['/admin']);
        } else {
          this.router.navigateByUrl(this.returnUrl);
        }
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Invalid email or password. Please try again.';
        this.changeDetector.markForCheck();
      }
    });
  }

  onRegister(): void {
    if (!this.regFullName || !this.regEmail || !this.regPassword) {
      this.errorMessage = 'Please fill out all required fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.register({
      fullName: this.regFullName,
      email: this.regEmail,
      password: this.regPassword,
      phoneNumber: this.regPhone,
      address: this.regAddress
    }).subscribe({
      next: () => {
        this.isLoading = false;
        this.router.navigateByUrl(this.returnUrl);
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Registration failed. Email might already be registered.';
        this.changeDetector.markForCheck();
      }
    });
  }
}
