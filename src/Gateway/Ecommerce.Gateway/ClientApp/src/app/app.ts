import { Component, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { GatewayApiService } from './core/services/gateway-api.service';
import { ConsoleSessionService } from './core/services/console-session.service';

@Component({
  selector: 'app-root',
  imports: [NgClass, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly session = inject(ConsoleSessionService);
  protected readonly api = inject(GatewayApiService);
  protected readonly summary = this.session.summary;
  protected readonly navItems = [
    {
      path: '/dashboard',
      label: 'Dashboard',
      description: 'Run the full checkout flow and open diagnostics.',
    },
    {
      path: '/basket',
      label: 'Basket',
      description: 'Create baskets, edit items, and trigger checkout.',
    },
    {
      path: '/orders',
      label: 'Orders',
      description: 'Inspect the order state after checkout.',
    },
    {
      path: '/logistics',
      label: 'Logistics',
      description: 'Follow shipment creation and failures.',
    },
  ] as const;

  protected formatPayload(payload: unknown): string {
    if (typeof payload === 'string') {
      return payload;
    }

    return JSON.stringify(payload ?? 'No content', null, 2);
  }
}
