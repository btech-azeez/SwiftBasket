import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ProductCardComponent } from '../../components/product-card/product-card';
import { ProductService } from '../../services/product.service';
import { CategoryService } from '../../services/category.service';
import { Category, Product } from '../../models/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ProductCardComponent],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class HomeComponent implements OnInit {
  private changeDetector = inject(ChangeDetectorRef);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private route = inject(ActivatedRoute);

  categories: Category[] = [];
  products: Product[] = [];
  isLoading = true;

  selectedCategoryId: number | null = null;
  searchQuery: string = '';
  sortBy: string = 'featured';

  ngOnInit(): void {
    this.loadCategories();

    this.route.queryParams.subscribe(params => {
      this.searchQuery = params['q'] || '';
      if (params['category']) {
        this.selectedCategoryId = +params['category'];
      }
      this.loadProducts();
    });
  }

  loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        console.error('Failed to load categories', err);
        this.changeDetector.markForCheck();
      }
    });
  }

  loadProducts(): void {
    this.isLoading = true;
    this.productService.getProducts({
      search: this.searchQuery,
      categoryId: this.selectedCategoryId || undefined,
      sortBy: this.sortBy
    }).subscribe({
      next: (data) => {
        this.products = data;
        this.isLoading = false;
        this.changeDetector.markForCheck();
      },
      error: (err) => {
        console.error('Failed to load products', err);
        this.isLoading = false;
        this.changeDetector.markForCheck();
      }
    });
  }

  selectCategory(catId: number | null): void {
    this.selectedCategoryId = catId;
    this.loadProducts();
  }

  onSortChange(): void {
    this.loadProducts();
  }

  clearAllFilters(): void {
    this.selectedCategoryId = null;
    this.searchQuery = '';
    this.sortBy = 'featured';
    this.loadProducts();
  }
}
