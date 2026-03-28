# Event Schemas

## Base Fields

All events include:

- `eventId`
- `correlationId`
- `causationId`
- `timestamp`
- `version`

## Events

### BasketCheckedOut

```json
{
  "basketId": "basket-1",
  "customerId": "customer-1",
  "items": [
    { "productId": "product-1", "productName": "Keyboard", "quantity": 2, "unitPrice": 79.99 }
  ],
  "itemsTotal": 159.98
}
```

### OrderCreated

```json
{
  "orderId": "order-1",
  "customerId": "customer-1",
  "itemsTotal": 159.98,
  "status": "Pending"
}
```

### OrderConfirmed

```json
{
  "orderId": "order-1",
  "shippingPrice": 12.50,
  "finalTotal": 172.48,
  "status": "Confirmed"
}
```

### OrderCancelled

```json
{
  "orderId": "order-1",
  "reason": "Shipment failed",
  "status": "Cancelled"
}
```

### ShipmentReservationRequested

```json
{
  "orderId": "order-1"
}
```

### ShipmentReserved

```json
{
  "orderId": "order-1",
  "shipmentId": "shipment-1",
  "carrier": "DefaultCarrier"
}
```

### ShipmentRepriced

```json
{
  "orderId": "order-1",
  "shipmentId": "shipment-1",
  "shippingPrice": 12.50
}
```

### ShipmentFailed

```json
{
  "orderId": "order-1",
  "reason": "No shipment available"
}
```

## Flow

`BasketCheckedOut -> OrderCreated -> ShipmentReserved/ShipmentFailed -> OrderConfirmed/OrderCancelled`
