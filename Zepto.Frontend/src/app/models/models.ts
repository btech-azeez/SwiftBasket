export interface User {
  id: number;
  fullName: string;
  email: string;
  role: 'Customer' | 'Admin';
  phoneNumber?: string;
  address?: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface Category {
  id: number;
  name: string;
  description?: string;
  imageUrl?: string;
  icon?: string;
  productCount: number;
}

export interface Product {
  id: number;
  name: string;
  description?: string;
  price: number;
  discountPrice?: number;
  stockQuantity: number;
  unit: string;
  imageUrl?: string;
  inStock: boolean;
  isFeatured: boolean;
  categoryId: number;
  categoryName: string;
}

export interface CartItem {
  product: Product;
  quantity: number;
}

export interface OrderItem {
  id: number;
  productId: number;
  productName: string;
  productImageUrl?: string;
  unit: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface Order {
  id: number;
  userId: number;
  customerName: string;
  customerEmail: string;
  orderDate: string;
  totalAmount: number;
  deliveryFee: number;
  status: 'Placed' | 'Processing' | 'Out for Delivery' | 'Delivered' | 'Cancelled';
  shippingAddress: string;
  phoneNumber: string;
  paymentMethod: string;
  paymentStatus: string;
  items: OrderItem[];
}

export interface CreateOrderItemDto {
  productId: number;
  quantity: number;
}

export interface CreateOrderDto {
  shippingAddress: string;
  phoneNumber: string;
  paymentMethod: string;
  items: CreateOrderItemDto[];
}

export interface DashboardSummary {
  totalRevenue: number;
  totalOrders: number;
  totalProducts: number;
  totalCustomers: number;
  pendingOrders: number;
  deliveredOrders: number;
  recentOrders: Order[];
}
