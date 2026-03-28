# Logistics Agent - Context

## Purpose

The Logistics service manages simple shipment state for orders.

## Minimal Model

### Shipment

- `shipmentId`
- `orderId`
- `carrier`
- `shippingPrice`
- `status`

### Statuses

- `Requested`
- `Reserved`
- `Shipped`
- `Failed`

## Rules

- Create a shipment when an order is created
- Use a simple default carrier name such as `DefaultCarrier`
- Use a simple shipping price
- If shipment creation fails, publish `ShipmentFailed`
- If shipment creation succeeds, publish `ShipmentReserved`

## Events

Consumes:

- `OrderCreated`
- `OrderCancelled`

Publishes:

- `ShipmentReservationRequested`
- `ShipmentReserved`
- `ShipmentRepriced`
- `ShipmentFailed`
