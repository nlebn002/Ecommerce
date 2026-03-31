import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  BasketDto,
  CreateBasketRequest,
  OrderDetailsDto,
  OrderSummaryDto,
  ShipmentDetailsDto,
  UpsertBasketItemRequest,
} from '../models/contracts';
import { RequestHistoryEntry, ResponseState } from '../models/console.models';
import { ConsoleSessionService } from './console-session.service';

@Injectable({ providedIn: 'root' })
export class GatewayApiService {
  private readonly http = inject(HttpClient);
  private readonly session = inject(ConsoleSessionService);

  readonly busy = signal(false);
  readonly lastResponse = signal<ResponseState>({
    label: 'Idle',
    status: '',
    payload: 'Ready.',
  });
  readonly history = signal<RequestHistoryEntry[]>([]);

  async createBasket(customerId: string) {
    return this.perform<BasketDto>('POST', '/basket/v1/baskets', { customerId });
  }

  async getBasket(basketId: string) {
    return this.perform<BasketDto>('GET', `/basket/v1/baskets/${basketId}`);
  }

  async upsertBasketItem(basketId: string, request: UpsertBasketItemRequest) {
    return this.perform<BasketDto>('PUT', `/basket/v1/baskets/${basketId}/items`, request);
  }

  async removeBasketItem(basketId: string, productId: string) {
    return this.perform<BasketDto>('DELETE', `/basket/v1/baskets/${basketId}/items/${productId}`);
  }

  async checkoutBasket(basketId: string) {
    return this.perform<BasketDto>('POST', `/basket/v1/baskets/${basketId}/checkout`);
  }

  async getOrdersByCustomer(customerId: string) {
    return this.perform<OrderSummaryDto[]>('GET', `/orders/v1/orders/by-customer/${customerId}`);
  }

  async getOrder(orderId: string) {
    return this.perform<OrderDetailsDto>('GET', `/orders/v1/orders/${orderId}`);
  }

  async getShipmentByOrder(orderId: string) {
    return this.perform<ShipmentDetailsDto>('GET', `/logistics/v1/shipments/by-order/${orderId}`);
  }

  async getShipment(shipmentId: string) {
    return this.perform<ShipmentDetailsDto>('GET', `/logistics/v1/shipments/${shipmentId}`);
  }

  async runScenario(createRequest: CreateBasketRequest, item: UpsertBasketItemRequest) {
    this.session.customerId.set(createRequest.customerId);
    this.session.resetFlow();

    const basket = await this.createBasket(createRequest.customerId);
    this.updateFromBasket(basket);

    await this.upsertBasketItem(basket.basketId, item);
    await this.checkoutBasket(basket.basketId);
    await this.delay(1500);

    const orders = await this.getOrdersByCustomer(createRequest.customerId);
    this.updateFromOrders(orders);

    if (this.session.orderId()) {
      await this.getOrder(this.session.orderId());
      await this.delay(1000);
      const shipment = await this.getShipmentByOrder(this.session.orderId());
      this.updateFromShipment(shipment);
    }
  }

  clearOutput() {
    this.lastResponse.set({
      label: 'Idle',
      status: '',
      payload: 'Ready.',
    });
    this.history.set([]);
  }

  private async perform<T>(method: string, path: string, body?: unknown): Promise<T> {
    this.busy.set(true);
    this.lastResponse.set({
      label: `${method} ${path}`,
      status: 'working',
      payload: 'Request in flight...',
    });

    const startedAt = new Date();

    try {
      const response = await firstValueFrom(
        this.http.request<T>(method, path, {
          body,
          observe: 'response',
        }),
      );

      const payload = response.body as T;
      const label = `${method} ${path} -> ${response.status}`;

      this.lastResponse.set({
        label,
        status: 'ok',
        payload: payload ?? 'No content',
      });

      this.pushHistory({
        id: crypto.randomUUID(),
        label,
        startedAt: startedAt.toLocaleTimeString(),
        ok: true,
      });

      this.reconcileState(path, payload);
      return payload;
    } catch (error) {
      const failure = this.toFailure(error);

      this.lastResponse.set({
        label: failure.label,
        status: 'error',
        payload: failure.payload,
      });

      this.pushHistory({
        id: crypto.randomUUID(),
        label: failure.label,
        startedAt: startedAt.toLocaleTimeString(),
        ok: false,
      });

      throw error;
    } finally {
      this.busy.set(false);
    }
  }

  private reconcileState(path: string, payload: unknown) {
    if (path.startsWith('/basket/')) {
      this.updateFromBasket(payload as BasketDto);
      return;
    }

    if (path.includes('/by-customer/')) {
      this.updateFromOrders(payload as OrderSummaryDto[]);
      return;
    }

    if (path.startsWith('/orders/')) {
      const order = payload as OrderDetailsDto;
      this.session.orderId.set(order.orderId);
      return;
    }

    if (path.startsWith('/logistics/')) {
      this.updateFromShipment(payload as ShipmentDetailsDto);
    }
  }

  private updateFromBasket(basket: BasketDto | null | undefined) {
    if (!basket) {
      return;
    }

    this.session.basketId.set(basket.basketId);
    this.session.customerId.set(basket.customerId);
  }

  private updateFromOrders(orders: OrderSummaryDto[] | null | undefined) {
    const newest = orders?.[0];
    if (!newest) {
      return;
    }

    this.session.orderId.set(newest.orderId);
    this.session.customerId.set(newest.customerId);
  }

  private updateFromShipment(shipment: ShipmentDetailsDto | null | undefined) {
    if (!shipment) {
      return;
    }

    this.session.shipmentId.set(shipment.id);
    this.session.orderId.set(shipment.orderId);
  }

  private pushHistory(entry: RequestHistoryEntry) {
    this.history.update((entries) => [entry, ...entries].slice(0, 20));
  }

  private toFailure(error: unknown) {
    if (error instanceof HttpErrorResponse) {
      return {
        label: `${error.status ? `${error.status} ` : ''}${error.url ?? 'Request failed'}`.trim(),
        payload: error.error || error.message,
      };
    }

    return {
      label: 'Request failed',
      payload: error instanceof Error ? error.message : 'Unknown error',
    };
  }

  private delay(ms: number) {
    return new Promise((resolve) => window.setTimeout(resolve, ms));
  }
}
