import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ShipmentDetailsDto } from '../../core/models/contracts';
import { GatewayApiService } from '../../core/services/gateway-api.service';
import { ConsoleSessionService } from '../../core/services/console-session.service';

@Component({
  selector: 'app-logistics-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-grid">
      <article class="content-card span-6">
        <div class="section-head">
          <div>
            <span class="eyebrow">Logistics</span>
            <h3>Shipment queries</h3>
          </div>
          <p>Inspect shipment state by order id or direct shipment id.</p>
        </div>

        <form class="field-grid" [formGroup]="form">
          <div class="field">
            <label for="orderId">Order Id</label>
            <input id="orderId" type="text" formControlName="orderId" />
          </div>

          <div class="field">
            <label for="shipmentId">Shipment Id</label>
            <input id="shipmentId" type="text" formControlName="shipmentId" />
          </div>
        </form>

        <div class="action-row">
          <button class="button-primary" type="button" [disabled]="api.busy()" (click)="loadByOrder()">
            Get Shipment By Order
          </button>
          <button class="button-secondary" type="button" [disabled]="api.busy() || form.controls.shipmentId.invalid" (click)="loadShipment()">
            Get Shipment
          </button>
        </div>
      </article>

      <article class="content-card span-6">
        <div class="section-head">
          <div>
            <span class="eyebrow">Snapshot</span>
            <h3>Current shipment</h3>
          </div>
          <p>Good for verifying repricing, failure, and reservation state after order creation.</p>
        </div>

        @if (!shipment()) {
          <div class="empty-state">
            <div>No shipment loaded yet.</div>
            <div>Query by order id after the order is created.</div>
          </div>
        } @else {
          <div class="metric-grid">
            <div class="metric-tile">
              <span class="eyebrow">Shipment</span>
              <strong>{{ shipment()!.id }}</strong>
            </div>
            <div class="metric-tile">
              <span class="eyebrow">Status</span>
              <strong>{{ shipment()!.status }}</strong>
            </div>
            <div class="metric-tile">
              <span class="eyebrow">Carrier</span>
              <strong>{{ shipment()!.carrier }}</strong>
            </div>
            <div class="metric-tile">
              <span class="eyebrow">Shipping price</span>
              <strong>{{ shipment()!.shippingPrice | number: '1.2-2' }}</strong>
            </div>
          </div>

          @if (shipment()!.failureReason) {
            <div class="status-banner info">Failure reason: {{ shipment()!.failureReason }}</div>
          }
        }
      </article>
    </section>
  `,
})
export class LogisticsPageComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly api = inject(GatewayApiService);
  protected readonly session = inject(ConsoleSessionService);
  protected readonly shipment = signal<ShipmentDetailsDto | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    orderId: [this.session.orderId(), [Validators.required]],
    shipmentId: [this.session.shipmentId(), [Validators.required]],
  });

  protected async loadByOrder() {
    this.syncSession();
    const shipment = await this.api.getShipmentByOrder(this.form.getRawValue().orderId);
    this.shipment.set(shipment);
    this.form.patchValue({ shipmentId: shipment.id });
  }

  protected async loadShipment() {
    this.syncSession();
    const shipment = await this.api.getShipment(this.form.getRawValue().shipmentId);
    this.shipment.set(shipment);
  }

  private syncSession() {
    const value = this.form.getRawValue();
    this.session.orderId.set(value.orderId);
    this.session.shipmentId.set(value.shipmentId);
  }
}
