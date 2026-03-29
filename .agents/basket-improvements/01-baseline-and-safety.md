# Phase 1: Baseline And Safety

## Goal

Capture the current Basket runtime flow and establish a safe baseline before deeper refactoring.

## Scope

- Basket service only
- no broad architecture changes in this phase

## Tasks

1. Inspect and document the current execution flow:
   - API endpoint
   - application handler
   - domain aggregate
   - EF Core persistence
   - domain event dispatch
   - MassTransit publication
2. Add code comments only where the current behavior is not obvious.
3. Add or update tests around current checkout behavior if low-risk and helpful for later refactors.
4. Record the current risks found during analysis.

## Expected Output

- Clear notes on current Basket flow
- Minimal guardrail tests if feasible
- No unnecessary refactoring

## Acceptance Criteria

- A follow-up agent can understand the current request-to-message flow without redoing all analysis.
- Any added comments are short and high-signal.

## Likely Files

- `src/Services/Basket/Ecommerce.BasketService.Api/*`
- `src/Services/Basket/Ecommerce.BasketService.Application/*`
- `src/Services/Basket/Ecommerce.BasketService.Domain/*`
- `src/Services/Basket/Ecommerce.BasketService.Infrastructure/*`

## Constraints

- Do not implement outbox yet.
- Do not change API semantics yet.
