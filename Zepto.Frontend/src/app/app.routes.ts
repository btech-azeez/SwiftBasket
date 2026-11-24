import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home';
import { LoginComponent } from './pages/login/login';
import { CheckoutComponent } from './pages/checkout/checkout';
import { OrdersComponent } from './pages/orders/orders';
import { AdminComponent } from './pages/admin/admin';
import { adminGuard, authGuard } from './core/auth.guard';

export const routes: Routes = [
	{ path: '', component: HomeComponent, title: 'SwiftBasket | Groceries in minutes' },
	{ path: 'login', component: LoginComponent, title: 'Sign in | SwiftBasket' },
	{ path: 'checkout', component: CheckoutComponent, canActivate: [authGuard], title: 'Checkout | SwiftBasket' },
	{ path: 'orders', component: OrdersComponent, canActivate: [authGuard], title: 'My orders | SwiftBasket' },
	{ path: 'admin', component: AdminComponent, canActivate: [adminGuard], title: 'Admin dashboard | SwiftBasket' },
	{ path: '**', redirectTo: '' }
];
