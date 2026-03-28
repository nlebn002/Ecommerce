# Gateway - Context

## Architecture
     Client
       │
       ▼
   ┌────────┐
   │Gateway  │
   └──┬───┬──┘
      │   │
──────┴───┴──────── Message Broker (events) ───────
 │              │                │
 ▼              ▼                ▼
┌────────┐   ┌────────┐     ┌──────────┐
│Basket  │   │ Order  │     │Logistics │
│Service │   │Service │     │ Service  │
└────────┘   └────────┘     └──────────┘

## Event Flow

The core business flow is event-driven:
Customer checks out basket
Basket Service → publishes: basket.checked_out

Order Service consumes basket.checked_out → creates order
Order Service → publishes: order.confirmed

Logistics Service consumes order.confirmed → creates shipment
Logistics Service → publishes: shipment.shipped

Order Service consumes shipment.shipped → updates order status

Logistics Service delivers
Logistics Service → publishes: shipment.delivered

Order Service consumes shipment.delivered → marks order complete
## Events

| Event                | Producer   | Consumer(s)     | Payload                              |
|----------------------|------------|-----------------|--------------------------------------|
| basket.checked_out   | Basket     | Order           | basket_id, customer_id, items, total |
| order.confirmed      | Order      | Logistics       | order_id, items, shipping_address    |
| order.cancelled      | Order      | Logistics       | order_id, reason                     |
| shipment.shipped     | Logistics  | Order           | order_id, shipment_id, tracking_number |
| shipment.delivered   | Logistics  | Order           | order_id, shipment_id, delivered_at  |

## Synchronous Routes (Client-Facing)

| Method | Route                  | Service    |
|--------|------------------------|------------|
| GET    | /basket/:id            | Basket     |
| POST   | /basket/:id/items      | Basket     |
| POST   | /basket/:id/checkout   | Basket     |
| GET    | /orders/:id            | Order      |
| POST   | /orders/:id/cancel     | Order      |
| GET    | /orders/:id/tracking   | Logistics  |