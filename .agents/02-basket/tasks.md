# Basket Agent - Tasks

## Task 1: Get Basket

Load a basket by id. If it does not exist, create an empty active basket.

## Task 2: Add Or Update Item

Add an item to the basket or increase its quantity if it already exists.

Rules:

- reject quantity less than 1
- reject changes when basket status is `CheckedOut`
- recalculate total after the change

## Task 3: Remove Item

Remove one item from the basket.

Rules:

- if the item does not exist, return a validation error
- recalculate total after the change

## Task 4: Checkout Basket

Validate the basket is active and not empty, then mark it as checked out and publish `BasketCheckedOut`.
