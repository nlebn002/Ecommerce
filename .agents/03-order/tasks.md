# Order Agent - Tasks

## Task 1: Create Order From Basket Event

When `BasketCheckedOut` is received:

- create a new order
- copy customer and item data from the event
- set status to `Pending`
- publish `OrderCreated`

## Task 2: Confirm Or Cancel From Logistics Events

- when `ShipmentReserved` or `ShipmentRepriced` is received, set status to `Confirmed` and publish `OrderConfirmed`
- when `ShipmentFailed` is received, set status to `Cancelled` and publish `OrderCancelled`

## Task 3: Get Order

Return order details by `orderId`.

## Task 4: Cancel Order

Allow cancellation only when the order is not already shipped or cancelled.
