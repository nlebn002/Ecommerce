import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { GatewayApiService } from '../../core/services/gateway-api.service';
import { ConsoleSessionService } from '../../core/services/console-session.service';

@Component({
  selector: 'app-basket-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <section class="page-grid">
      <article class="content-card span-7">
        <div class="section-head">
          <div>
            <span class="eyebrow">Basket</span>
            <h3>Basket commands</h3>
          </div>
          <p>Create a basket, edit line items, then hand off to checkout.</p>
        </div>

        <form class="field-grid" [formGroup]="form">
          <div class="field">
            <label for="customerId">Customer Id</label>
            <input id="customerId" type="text" formControlName="customerId" />
          </div>

          <div class="field">
            <label for="basketId">Basket Id</label>
            <input id="basketId" type="text" formControlName="basketId" />
          </div>

          <div class="field">
            <label for="productId">Product Id</label>
            <input id="productId" type="text" formControlName="productId" />
          </div>

          <div class="field">
            <label for="productName">Product Name</label>
            <input id="productName" type="text" formControlName="productName" />
          </div>

          <div class="field">
            <label for="quantity">Quantity</label>
            <input id="quantity" type="number" formControlName="quantity" />
          </div>

          <div class="field">
            <label for="unitPrice">Unit Price</label>
            <input id="unitPrice" type="number" step="0.01" formControlName="unitPrice" />
          </div>
        </form>

        <div class="action-row">
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="newCustomer()">New Customer</button>
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="newItem()">Random Item</button>
          <button class="button-primary" type="button" [disabled]="api.busy()" (click)="createBasket()">Create Basket</button>
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="getBasket()">Get Basket</button>
          <button class="button-primary" type="button" [disabled]="api.busy() || form.invalid" (click)="upsertItem()">Add Or Update Item</button>
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="removeItem()">Remove Item</button>
          <button class="button-primary" type="button" [disabled]="api.busy()" (click)="checkout()">Checkout</button>
        </div>

        <div class="status-banner success" *ngIf="session.basketId()">
          Active basket id: {{ session.basketId() }}
        </div>
      </article>

      <article class="content-card span-5">
        <div class="section-head">
          <div>
            <span class="eyebrow">Guide</span>
            <h3>Fast manual path</h3>
          </div>
          <p>The UI keeps ids in memory, so each action fills the next step automatically.</p>
        </div>

        <div class="empty-state">
          <div>1. Generate a customer and random item.</div>
          <div>2. Create the basket.</div>
          <div>3. Add or update the item.</div>
          <div>4. Checkout and switch to Orders.</div>
        </div>
      </article>
    </section>
  `,
})
export class BasketPageComponent {
  private readonly fb = inject(FormBuilder);
  protected readonly api = inject(GatewayApiService);
  protected readonly session = inject(ConsoleSessionService);

  protected readonly form = this.fb.nonNullable.group({
    customerId: [this.session.customerId(), [Validators.required]],
    basketId: [this.session.basketId()],
    productId: ['', [Validators.required]],
    productName: ['', [Validators.required]],
    quantity: [1, [Validators.required, Validators.min(1)]],
    unitPrice: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    this.newItem();
  }

  protected newCustomer() {
    this.form.patchValue({ customerId: this.session.regenerateCustomer() });
  }

  protected newItem() {
    const item = this.session.regenerateItem();
    this.form.patchValue(item);
  }

  protected async createBasket() {
    this.syncSession();
    const basket = await this.api.createBasket(this.form.getRawValue().customerId);
    this.form.patchValue({ basketId: basket.basketId, customerId: basket.customerId });
  }

  protected async getBasket() {
    this.syncSession();
    const basket = await this.api.getBasket(this.form.getRawValue().basketId);
    this.form.patchValue({ basketId: basket.basketId, customerId: basket.customerId });
  }

  protected async upsertItem() {
    this.syncSession();
    const { basketId, productId, productName, quantity, unitPrice } = this.form.getRawValue();
    await this.api.upsertBasketItem(basketId, {
      productId,
      productName,
      quantity,
      unitPrice,
    });
  }

  protected async removeItem() {
    this.syncSession();
    const { basketId, productId } = this.form.getRawValue();
    await this.api.removeBasketItem(basketId, productId);
  }

  protected async checkout() {
    this.syncSession();
    await this.api.checkoutBasket(this.form.getRawValue().basketId);
  }

  private syncSession() {
    const value = this.form.getRawValue();
    this.session.customerId.set(value.customerId);
    this.session.basketId.set(value.basketId);
  }
}
