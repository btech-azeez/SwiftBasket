import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavbarComponent } from './components/navbar/navbar';
import { CartDrawerComponent } from './components/cart-drawer/cart-drawer';

@Component({
  imports: [RouterOutlet, NavbarComponent, CartDrawerComponent],
  selector: 'app-root',
  styleUrl: './shell.css',
  templateUrl: './shell.html',
})
export class App {
}
