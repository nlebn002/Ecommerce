import { Component, inject } from '@angular/core';
import { GatewayApiService } from '../../core/services/gateway-api.service';
import { ConsoleSessionService } from '../../core/services/console-session.service';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  template: `
    <section class="page-grid">
      <article class="content-card span-7">
        <div class="section-head">
          <div>
            <span class="eyebrow">Scenario</span>
            <h3>Full flow driver</h3>
          </div>
          <p>Generate a customer, create a basket, add an item, checkout, then inspect order and shipment state.</p>
        </div>

        <div class="action-row">
          <button class="button-primary" type="button" [disabled]="api.busy()" (click)="runScenario()">
            {{ api.busy() ? 'Running...' : 'Run Full Scenario' }}
          </button>
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="newCustomer()">
            New Customer
          </button>
          <button class="button-secondary" type="button" [disabled]="api.busy()" (click)="newItem()">
            New Random Item
          </button>
          <button class="button-ghost" type="button" [disabled]="api.busy()" (click)="api.clearOutput()">
            Clear Output
          </button>
        </div>

        <div class="status-banner info">
          This route is the fastest way to produce useful traces and logs across the full microservice chain.
        </div>
      </article>

      <article class="content-card span-5">
        <div class="section-head">
          <div>
            <span class="eyebrow">Context</span>
            <h3>Live pointers</h3>
          </div>
          <p>Use these ids in the feature routes without retyping them.</p>
        </div>

        <div class="metric-grid">
          @for (item of session.summary(); track item.label) {
            <div class="metric-tile">
              <span class="eyebrow">{{ item.label }}</span>
              <strong>{{ shorten(item.value) }}</strong>
            </div>
          }
        </div>
      </article>

      <article class="content-card span-12">
        <div class="section-head">
          <div>
            <span class="eyebrow">Sequence</span>
            <h3>Recommended path</h3>
          </div>
          <p>Stay in the gateway UI and drop to API docs only when you need schema detail.</p>
        </div>

        <div class="empty-state">
          <div>1. Create a fresh customer and item seed.</div>
          <div>2. Run the full scenario once to generate order and shipment ids.</div>
          <div>3. Use Basket, Orders, and Logistics for targeted retests.</div>
          <div>4. Open Scalar or Metrics if the backend flow does not match the UI state.</div>
        </div>
      </article>
    </section>
  `,
})
export class DashboardPageComponent {
  protected readonly api = inject(GatewayApiService);
  protected readonly session = inject(ConsoleSessionService);

  protected async runScenario() {
    const customerId = this.session.regenerateCustomer();
    const item = this.session.regenerateItem();

    try {
      await this.api.runScenario({ customerId }, item);
    } catch {
      return;
    }
  }

  protected newCustomer() {
    this.session.regenerateCustomer();
  }

  protected newItem() {
    this.session.regenerateItem();
  }

  protected shorten(value: string) {
    return value.length > 18 ? `${value.slice(0, 8)}...${value.slice(-4)}` : value;
  }
}
