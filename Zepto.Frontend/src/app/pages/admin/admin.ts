import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { forkJoin, map } from 'rxjs';
import { Category, DashboardSummary, Order, Product } from '../../models/models';
import { CategoryService } from '../../services/category.service';
import { DashboardService } from '../../services/dashboard.service';
import { OrderService } from '../../services/order.service';
import { ProductService } from '../../services/product.service';

type AdminTab = 'overview' | 'products' | 'categories' | 'orders';

interface ProductForm {
  name: string;
  description: string;
  price: number;
  discountPrice: number | null;
  stockQuantity: number;
  unit: string;
  imageUrl: string;
  isFeatured: boolean;
  categoryId: number;
}

interface CategoryForm {
  name: string;
  description: string;
  imageUrl: string;
  icon: string;
}

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin.html',
  styleUrl: './admin.css'
})
export class AdminComponent implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private dashboardService = inject(DashboardService);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private orderService = inject(OrderService);

  activeTab: AdminTab = 'overview';
  summary: DashboardSummary | null = null;
  products: Product[] = [];
  categories: Category[] = [];
  orders: Order[] = [];
  isLoading = true;
  isSaving = false;
  errorMessage = '';
  notice = '';
  editingProductId: number | null = null;
  editingCategoryId: number | null = null;
  productForm = this.emptyProductForm();
  categoryForm = this.emptyCategoryForm();
  readonly orderStatuses = ['Placed', 'Processing', 'Out for Delivery', 'Delivered', 'Cancelled'];
  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';
    forkJoin({
      summary: this.dashboardService.getSummary(),
      products: this.productService.getProducts(),
      categories: this.categoryService.getCategories(),
      orders: this.orderService.getAllOrders()
    }).subscribe({
      next: (data) => {
        this.summary = data.summary;
        this.products = data.products;
        this.categories = data.categories;
        this.orders = data.orders;
        this.isLoading = false;
        if (!this.productForm.categoryId && this.categories.length) {
          this.productForm.categoryId = this.categories[0].id;
        }
        this.changeDetector.markForCheck();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not load admin data. Check the API and your admin access.';
        this.isLoading = false;
        this.changeDetector.markForCheck();
      }
    });
  }

  setTab(tab: AdminTab): void {
    this.activeTab = tab;
    this.notice = '';
    this.errorMessage = '';
  }

  editProduct(product: Product): void {
    this.activeTab = 'products';
    this.editingProductId = product.id;
    this.productForm = {
      name: product.name,
      description: product.description || '',
      price: product.price,
      discountPrice: product.discountPrice ?? null,
      stockQuantity: product.stockQuantity,
      unit: product.unit,
      imageUrl: product.imageUrl || '',
      isFeatured: product.isFeatured,
      categoryId: product.categoryId
    };
  }

  saveProduct(): void {
    if (!this.productForm.categoryId) {
      this.errorMessage = 'Create a category before adding products.';
      return;
    }

    const payload: Partial<Product> = {
      ...this.productForm,
      discountPrice: this.productForm.discountPrice ?? undefined,
      inStock: this.productForm.stockQuantity > 0
    };
    this.isSaving = true;
    const request = this.editingProductId
      ? this.productService.updateProduct(this.editingProductId, payload).pipe(map(() => undefined))
      : this.productService.createProduct(payload).pipe(map(() => undefined));

    request.subscribe({
      next: () => {
        this.notice = this.editingProductId ? 'Product updated.' : 'Product created.';
        this.cancelProductEdit();
        this.isSaving = false;
        this.changeDetector.markForCheck();
        this.loadData();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not save product.';
        this.isSaving = false;
        this.changeDetector.markForCheck();
      }
    });
  }

  cancelProductEdit(): void {
    this.editingProductId = null;
    this.productForm = this.emptyProductForm();
    if (this.categories.length) this.productForm.categoryId = this.categories[0].id;
  }

  deleteProduct(product: Product): void {
    if (!window.confirm(`Delete ${product.name}?`)) return;
    this.productService.deleteProduct(product.id).subscribe({
      next: () => {
        this.notice = 'Product deleted.';
        this.changeDetector.markForCheck();
        this.loadData();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not delete product.';
        this.changeDetector.markForCheck();
      }
    });
  }

  editCategory(category: Category): void {
    this.activeTab = 'categories';
    this.editingCategoryId = category.id;
    this.categoryForm = {
      name: category.name,
      description: category.description || '',
      imageUrl: category.imageUrl || '',
      icon: category.icon || ''
    };
  }

  saveCategory(): void {
    this.isSaving = true;
    const request = this.editingCategoryId
      ? this.categoryService.updateCategory(this.editingCategoryId, this.categoryForm).pipe(map(() => undefined))
      : this.categoryService.createCategory(this.categoryForm).pipe(map(() => undefined));

    request.subscribe({
      next: () => {
        this.notice = this.editingCategoryId ? 'Category updated.' : 'Category created.';
        this.cancelCategoryEdit();
        this.isSaving = false;
        this.changeDetector.markForCheck();
        this.loadData();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not save category.';
        this.isSaving = false;
        this.changeDetector.markForCheck();
      }
    });
  }

  cancelCategoryEdit(): void {
    this.editingCategoryId = null;
    this.categoryForm = this.emptyCategoryForm();
  }

  deleteCategory(category: Category): void {
    if (!window.confirm(`Delete ${category.name}?`)) return;
    this.categoryService.deleteCategory(category.id).subscribe({
      next: () => {
        this.notice = 'Category deleted.';
        this.changeDetector.markForCheck();
        this.loadData();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not delete category.';
        this.changeDetector.markForCheck();
      }
    });
  }

  updateOrderStatus(order: Order, status: string): void {
    this.orderService.updateOrderStatus(order.id, status).subscribe({
      next: () => {
        this.notice = `Order #${order.id} updated.`;
        this.changeDetector.markForCheck();
        this.loadData();
      },
      error: (error: any) => {
        this.errorMessage = error.error?.message || 'Could not update order status.';
        this.changeDetector.markForCheck();
      }
    });
  }

  private emptyProductForm(): ProductForm {
    return {
      name: '', description: '', price: 0.01, discountPrice: null,
      stockQuantity: 0, unit: '1 unit', imageUrl: '', isFeatured: false, categoryId: 0
    };
  }

  private emptyCategoryForm(): CategoryForm {
    return { name: '', description: '', imageUrl: '', icon: '' };
  }
}