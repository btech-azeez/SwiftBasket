import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-cart-drawer',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './cart-drawer.html',
  styleUrl: './cart-drawer.css'
})
export class CartDrawerComponent {
  cartService = inject(CartService);
  authService = inject(AuthService);
  private router = inject(Router);

  close(): void {
    this.cartService.closeCart();
  }

  proceedToCheckout(): void {
    this.cartService.closeCart();
    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login'], { queryParams: { returnUrl: '/checkout' } });
    } else {
      this.router.navigate(['/checkout']);
    }
  }

  get remainingForFreeDelivery(): number {
    const sub = this.cartService.subtotal();
    const threshold = this.cartService.freeDeliveryThreshold;
    return Math.max(0, threshold - sub);
  }

  get freeDeliveryProgressPercent(): number {
    const sub = this.cartService.subtotal();
    const threshold = this.cartService.freeDeliveryThreshold;
    return Math.min(100, Math.round((sub / threshold) * 100));
  }
}
