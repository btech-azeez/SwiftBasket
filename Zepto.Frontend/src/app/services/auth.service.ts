import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { AuthResponse, User } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/auth';

  private userSignal = signal<User | null>(this.getStoredUser());
  private tokenSignal = signal<string | null>(localStorage.getItem('zepto_token'));

  // Public computed signals
  readonly currentUser = this.userSignal.asReadonly();
  readonly token = this.tokenSignal.asReadonly();
  readonly isLoggedIn = computed(() => !!this.tokenSignal());
  readonly isAdmin = computed(() => this.userSignal()?.role === 'Admin');

  constructor() {
    if (this.tokenSignal()) {
      this.fetchCurrentUser().subscribe({
        error: () => this.logout()
      });
    }
  }

  private getStoredUser(): User | null {
    const raw = localStorage.getItem('zepto_user');
    if (!raw) return null;
    try {
      return JSON.parse(raw) as User;
    } catch {
      return null;
    }
  }

  register(dto: { fullName: string; email: string; password: string; phoneNumber?: string; address?: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, dto).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  login(credentials: { email: string; password: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => this.handleAuthSuccess(res))
    );
  }

  fetchCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/me`).pipe(
      tap(user => {
        this.userSignal.set(user);
        localStorage.setItem('zepto_user', JSON.stringify(user));
      })
    );
  }

  logout(): void {
    localStorage.removeItem('zepto_token');
    localStorage.removeItem('zepto_user');
    this.tokenSignal.set(null);
    this.userSignal.set(null);
  }

  private handleAuthSuccess(res: AuthResponse): void {
    localStorage.setItem('zepto_token', res.token);
    localStorage.setItem('zepto_user', JSON.stringify(res.user));
    this.tokenSignal.set(res.token);
    this.userSignal.set(res.user);
  }
}
