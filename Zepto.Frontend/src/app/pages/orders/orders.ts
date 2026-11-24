import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { OrderService } from '../../services/order.service';
import { Order } from '../../models/models';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css'
})
export class OrdersComponent implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private orderService = inject(OrderService);
  private route = inject(ActivatedRoute);

  orders: Order[] = [];
  isLoading = true;
  successOrderId: string | null = null;

  ngOnInit(): void {
    this.successOrderId = this.route.snapshot.queryParams['success'] || null;
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.orderService.getMyOrders().subscribe({
      next: (data) => {
        this.orders = data;
        this.isLoading = false;
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        console.error('Failed to load orders', err);
        this.isLoading = false;
        this.changeDetector.markForCheck();
      }
    });
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'delivered': return 'status-delivered';
      case 'out for delivery': return 'status-out';
      case 'processing': return 'status-processing';
      case 'placed': return 'status-placed';
      case 'cancelled': return 'status-cancelled';
      default: return 'status-placed';
    }
  }
}
