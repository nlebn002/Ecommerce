# Basket Agent - Context

## Purpose

The Basket service stores a customer's current shopping basket.

## Minimal Model

### Basket

- `basketId`
- `customerId`
- `items`
- `total`
- `status`: `Active` or `CheckedOut`

### Basket Item

- `productId`
- `productName`
- `quantity`
- `unitPrice`

## Rules

- Quantity must be greater than zero
- If the same product is added twice, increase quantity
- Total is the sum of `quantity * unitPrice`
- Checked out baskets cannot be changed

## Event

On checkout, publish `BasketCheckedOut` with:

- `basketId`
- `customerId`
- `items`
- `itemsTotal`
