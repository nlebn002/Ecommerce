import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrderSummaryDto } from '../../core/models/contracts';
import { GatewayApiService } from '../../core/services/gateway-api.service';
import { ConsoleSessionService } from '../../core/services/console-session.service';

@Component({
  selector: 'app-orders-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-grid">
      <article class="content-card span-6">
        <div class="section-head">
          <div>
            <span class="eyebrow">Orders</span>
            <h3>Order queries</h3>
          </div>
          <p>Load a customer history or inspect one order directly.</p>
        </div>

        <form class="field-grid" [formGroup]="form">
          <div class="field">
            <label for="customerId">Customer Id</label>
            <input id="customerId" type="text" formControlName="customerId" />
          </div>

          <div class="field">
            <label for="orderId">Order Id</label>
            <input id="orderId" type="text" formControlName="orderId" />
          </div>
        </form>

        <div class="action-row">
          <button class="button-primary" type="button" [disabled]="api.busy()" (click)="loadOrders()">
            Get Customer Orders
          </button>
          <button class="button-secondary" type="button" [disabled]="api.busy() || form.controls.orderId.invalid" (click)="loadOrder()">
            Get Order
          </button>
        </div>
      </article>

      <article class="content-card span-6">
        <div class="section-head">
          <div>
            <span class="eyebrow">Signal</span>
            <h3>Recent customer orders</h3>
          </div>
          <p>Use the latest order id to jump straight to Logistics.</p>
        </div>

        @if (orders().length === 0) {
          <div class="empty-state">
            <div>No orders loaded yet.</div>
            <div>Run the dashboard scenario or checkout a basket first.</div>
          </div>
        } @else {
          <div class="table-grid">
            @for (order of orders(); track order.orderId) {
              <div class="table-row">
                <div>
                  <span class="eyebrow">Order</span>
                  <strong>{{ order.orderId }}</strong>
                </div>
                <div>
                  <span class="eyebrow">Status</span>
                  <strong>{{ order.status }}</strong>
                </div>
                <div>
                  <span class="eyebrow">Shipping</span>
                  <strong>{{ order.shippingPrice | number: '1.2-2' }}</strong>
                </div>
                <div>
                  <span class="eyebrow">Final total</span>
                  <strong>{{ order.finalTotal | number: '1.2-2' }}</strong>
                </div>
              </div>
            }
          </div>
        }
      </article>
    </section>
  `,
})
export class OrdersPageComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly api = inject(GatewayApiService);
  protected readonly session = inject(ConsoleSessionService);
  protected readonly orders = signal<OrderSummaryDto[]>([]);

  protected readonly form = this.fb.nonNullable.group({
    customerId: [this.session.customerId(), [Validators.required]],
    orderId: [this.session.orderId(), [Validators.required]],
  });

  protected async loadOrders() {
    this.syncSession();
    const orders = await this.api.getOrdersByCustomer(this.form.getRawValue().customerId);
    this.orders.set(orders);

    if (orders[0]) {
      this.form.patchValue({ orderId: orders[0].orderId });
    }
  }

  protected async loadOrder() {
    this.syncSession();
    await this.api.getOrder(this.form.getRawValue().orderId);
  }

  private syncSession() {
    const value = this.form.getRawValue();
    this.session.customerId.set(value.customerId);
    this.session.orderId.set(value.orderId);
  }
}
