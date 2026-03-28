# Order Agent - Context

## Purpose

The Order service creates and tracks orders after basket checkout.

## Minimal Model

### Order

- `orderId`
- `customerId`
- `items`
- `total`
- `status`

### Statuses

- `Pending`
- `Confirmed`
- `Shipped`
- `Cancelled`

## Rules

- Create an order when `BasketCheckedOut` is received
- New orders start as `Pending`
- After shipment is reserved or confirmed, order can move to `Confirmed`
- After shipment is sent, order moves to `Shipped`
- Cancel is only allowed before shipment

## Events

Consumes:

- `BasketCheckedOut`
- `ShipmentReserved`
- `ShipmentFailed`

Publishes:

- `OrderCreated`
- `OrderConfirmed`
- `OrderCancelled`
