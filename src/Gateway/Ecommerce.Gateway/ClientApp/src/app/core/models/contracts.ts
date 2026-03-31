export interface BasketItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface BasketDto {
  basketId: string;
  customerId: string;
  status: string;
  total: number;
  items: BasketItemDto[];
}

export interface OrderSummaryDto {
  orderId: string;
  customerId: string;
  status: string;
  itemsTotal: number;
  shippingPrice: number;
  finalTotal: number;
}

export interface OrderLineItemDto {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface OrderDetailsDto {
  orderId: string;
  customerId: string;
  status: string;
  itemsTotal: number;
  shippingPrice: number;
  finalTotal: number;
  cancellationReason: string | null;
  items: OrderLineItemDto[];
}

export interface ShipmentDetailsDto {
  id: string;
  orderId: string;
  carrier: string;
  shippingPrice: number;
  status: string;
  failureReason: string | null;
}

export interface CreateBasketRequest {
  customerId: string;
}

export interface UpsertBasketItemRequest {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}
