# Coding Standards

## General

- One public type per file
- Prefer small files and methods
- No dead code, TODOs, or commented-out code
- Prefer composition over inheritance
- Use constants instead of magic values when helpful

## C# Style

- File-scoped namespaces
- Records for events, DTOs, and value objects
- Pattern matching where clear
- Target-typed `new` when obvious
- `Async` suffix on async methods

## Naming

- Private fields: `_camelCase`
- Interfaces: `IName`
- Events: past tense
- Handlers: `ActionEntityHandler`
- Endpoints: `FeatureEndpoints`
- Validators: `TypeValidator`
- Tests: `SutTests`

## Architecture

- Domain has no framework dependencies
- Application contains use cases
- Infrastructure contains integrations
- API exposes endpoints only

## Forbidden

- MediatR or similar mediator libraries
- Shared database between services
- Synchronous inter-service checkout flow
- Service locator
- `NotImplementedException`
