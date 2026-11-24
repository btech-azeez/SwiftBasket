import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cart.service';
import { AuthService } from '../../services/auth.service';
import { OrderService } from '../../services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class CheckoutComponent implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  cartService = inject(CartService);
  authService = inject(AuthService);
  private orderService = inject(OrderService);
  private router = inject(Router);

  shippingAddress = '';
  phoneNumber = '';
  paymentMethod = 'UPI'; // 'Cash on Delivery' | 'UPI' | 'Card'
  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    if (this.cartService.itemCount() === 0) {
      this.router.navigate(['/']);
      return;
    }

    const user = this.authService.currentUser();
    if (user) {
      this.shippingAddress = user.address || '42 Willow Street, Apt 3B, Bengaluru, Karnataka';
      this.phoneNumber = user.phoneNumber || '+91 98765 01928';
    }
  }

  placeOrder(): void {
    if (!this.shippingAddress.trim() || !this.phoneNumber.trim()) {
      this.errorMessage = 'Please provide both shipping address and phone number.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const items = this.cartService.items().map(i => ({
      productId: i.product.id,
      quantity: i.quantity
    }));

    this.orderService.createOrder({
      shippingAddress: this.shippingAddress.trim(),
      phoneNumber: this.phoneNumber.trim(),
      paymentMethod: this.paymentMethod,
      items
    }).subscribe({
      next: (order) => {
        this.isLoading = false;
        this.cartService.clearCart();
        this.router.navigate(['/orders'], { queryParams: { success: order.id } });
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || 'Failed to place order. Please try again.';
        this.changeDetector.markForCheck();
      }
    });
  }
}
