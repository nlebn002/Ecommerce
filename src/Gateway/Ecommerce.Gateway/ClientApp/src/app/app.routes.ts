import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () =>
      import('./features/dashboard/dashboard-page.component').then((m) => m.DashboardPageComponent),
  },
  {
    path: 'basket',
    loadComponent: () =>
      import('./features/basket/basket-page.component').then((m) => m.BasketPageComponent),
  },
  {
    path: 'orders',
    loadComponent: () =>
      import('./features/orders/orders-page.component').then((m) => m.OrdersPageComponent),
  },
  {
    path: 'logistics',
    loadComponent: () =>
      import('./features/logistics/logistics-page.component').then((m) => m.LogisticsPageComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
