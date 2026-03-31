import { computed, Injectable, signal } from '@angular/core';
import { BasketDraft, ProductSeed } from '../models/console.models';

const productCatalog: ProductSeed[] = [
  { name: 'Trail Mug', price: 19.99 },
  { name: 'Wool Throw', price: 49.5 },
  { name: 'Desk Lamp', price: 89 },
  { name: 'Travel Pack', price: 129.9 },
  { name: 'Field Notebook', price: 12.4 },
];

@Injectable({ providedIn: 'root' })
export class ConsoleSessionService {
  readonly customerId = signal(this.uuid());
  readonly basketId = signal('');
  readonly orderId = signal('');
  readonly shipmentId = signal('');

  readonly summary = computed(() => [
    { label: 'Gateway', value: window.location.origin },
    { label: 'Customer', value: this.customerId() || 'not set' },
    { label: 'Basket', value: this.basketId() || 'not set' },
    { label: 'Order', value: this.orderId() || 'not set' },
    { label: 'Shipment', value: this.shipmentId() || 'not set' },
  ]);

  regenerateCustomer(): string {
    const value = this.uuid();
    this.customerId.set(value);
    return value;
  }

  regenerateItem(): BasketDraft {
    const seed = productCatalog[Math.floor(Math.random() * productCatalog.length)];

    return {
      productId: this.uuid(),
      productName: seed.name,
      quantity: 1,
      unitPrice: Number(seed.price.toFixed(2)),
    };
  }

  resetFlow() {
    this.basketId.set('');
    this.orderId.set('');
    this.shipmentId.set('');
  }

  private uuid(): string {
    return crypto.randomUUID();
  }
}
