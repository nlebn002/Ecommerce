# Contracts Agent - Tasks

> Execute in order. Output every file with full content.

---

1. Generate `IIntegrationEvent.cs` with `EventId`, `CorrelationId`, `CausationId`, `Timestamp`, and `Version`
2. Generate `IntegrationEvent.cs` as a base record implementing `IIntegrationEvent`
3. Generate `V1/BasketItem.cs`
4. Generate `V1/BasketCheckedOut.cs`
5. Generate `V1/OrderCreated.cs`
6. Generate `V1/OrderConfirmed.cs`
7. Generate `V1/OrderCancelled.cs`
8. Generate `V1/ShipmentReservationRequested.cs`
9. Generate `V1/ShipmentReserved.cs`
10. Generate `V1/ShipmentRepriced.cs`
11. Generate `V1/ShipmentFailed.cs`
12. Generate `V2/BasketItem.cs` with additive fields such as `Sku` and `Weight`
13. Generate `V2/BasketCheckedOut.cs` using the V2 basket item

## Done When

- Every event is a record type
- Every event implements `IIntegrationEvent`
- V2 is additive only over V1
- The Contracts project builds successfully
