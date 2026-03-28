# Logistics Agent - Tasks

## Task 1: Create Shipment For Order

When `OrderCreated` is received:

- create a shipment record
- set status to `Reserved`
- assign a simple carrier and shipping price
- publish `ShipmentReserved`
- optionally publish `ShipmentRepriced` if shipping price is part of the order flow

## Task 2: Fail Shipment

If shipment creation cannot be completed, set status to `Failed` and publish `ShipmentFailed`.

## Task 3: Mark Shipment As Shipped

Update a reserved shipment to `Shipped`.

## Task 4: Cancel Shipment

If an `OrderCancelled` event is received before shipping, stop processing that shipment.
