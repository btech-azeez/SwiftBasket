import { Injectable, signal, computed, effect } from '@angular/core';
import { CartItem, Product } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private itemsSignal = signal<CartItem[]>(this.loadCartFromStorage());
  private isCartOpenSignal = signal<boolean>(false);

  readonly items = this.itemsSignal.asReadonly();
  readonly isCartOpen = this.isCartOpenSignal.asReadonly();

  readonly itemCount = computed(() => 
    this.itemsSignal().reduce((sum, item) => sum + item.quantity, 0)
  );

  readonly subtotal = computed(() =>
    this.itemsSignal().reduce((sum, item) => {
      const price = item.product.discountPrice ?? item.product.price;
      return sum + (price * item.quantity);
    }, 0)
  );

  readonly freeDeliveryThreshold = 2000;

  readonly deliveryFee = computed(() => {
    const sub = this.subtotal();
    if (sub === 0) return 0;
    return sub >= this.freeDeliveryThreshold ? 0 : 199;
  });

  readonly total = computed(() => this.subtotal() + this.deliveryFee());

  constructor() {
    effect(() => {
      localStorage.setItem('zepto_cart', JSON.stringify(this.itemsSignal()));
    });
  }

  private loadCartFromStorage(): CartItem[] {
    const raw = localStorage.getItem('zepto_cart');
    if (!raw) return [];
    try {
      const items = JSON.parse(raw) as CartItem[];
      return items.map(item => {
        if (item.product.price >= 100) return item;

        return {
          ...item,
          product: {
            ...item.product,
            price: item.product.price * 100,
            discountPrice: item.product.discountPrice == null ? undefined : item.product.discountPrice * 100
          }
        };
      });
    } catch {
      return [];
    }
  }

  openCart(): void {
    this.isCartOpenSignal.set(true);
  }

  closeCart(): void {
    this.isCartOpenSignal.set(false);
  }

  toggleCart(): void {
    this.isCartOpenSignal.update(open => !open);
  }

  addToCart(product: Product, quantity = 1): void {
    this.itemsSignal.update(items => {
      const existing = items.find(i => i.product.id === product.id);
      if (existing) {
        return items.map(i => 
          i.product.id === product.id 
            ? { ...i, quantity: i.quantity + quantity }
            : i
        );
      }
      return [...items, { product, quantity }];
    });
  }

  updateQuantity(productId: number, quantity: number): void {
    if (quantity <= 0) {
      this.removeFromCart(productId);
      return;
    }

    this.itemsSignal.update(items =>
      items.map(i => i.product.id === productId ? { ...i, quantity } : i)
    );
  }

  removeFromCart(productId: number): void {
    this.itemsSignal.update(items => items.filter(i => i.product.id !== productId));
  }

  getProductQuantity(productId: number): number {
    const item = this.itemsSignal().find(i => i.product.id === productId);
    return item ? item.quantity : 0;
  }

  clearCart(): void {
    this.itemsSignal.set([]);
  }
}
