import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Product } from '../../models/models';
import { CartService } from '../../services/cart.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css'
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;
  cartService = inject(CartService);

  get quantityInCart(): number {
    return this.cartService.getProductQuantity(this.product.id);
  }

  get discountPercent(): number | null {
    if (this.product.discountPrice && this.product.discountPrice < this.product.price) {
      return Math.round(((this.product.price - this.product.discountPrice) / this.product.price) * 100);
    }
    return null;
  }

  addToCart(): void {
    this.cartService.addToCart(this.product, 1);
  }

  increment(): void {
    this.cartService.updateQuantity(this.product.id, this.quantityInCart + 1);
  }

  decrement(): void {
    this.cartService.updateQuantity(this.product.id, this.quantityInCart - 1);
  }
}
