# Phase 7: Testing

## Goal

Add meaningful regression coverage for Basket domain logic, API behavior, and infrastructure flows.

## Scope

- Basket tests only

## Tasks

1. Add domain tests for:
   - add or update item
   - remove item
   - checkout invariants
   - total calculation
2. Add application tests for handlers.
3. Add integration tests for:
   - explicit create basket flow
   - read-only get flow
   - checkout writes outbox records
   - outbox publisher marks messages processed
4. Add failure-path tests where practical.

## Acceptance Criteria

- Basket has enough automated coverage to protect the critical paths.
- Tests are stable and readable.

## Likely Files

- `tests/*Basket*`
- test helpers and fixtures as needed

## Constraints

- Assume outbox and API cleanup phases are already implemented or coordinate closely with those agents.
