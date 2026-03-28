# Contracts Agent - Context

## Shared Knowledge

You have access to these shared files. Read and follow them:

- `shared/plan.md`
- `shared/tech-stack.md`
- `shared/coding-standards.md`
- `shared/project-structure.md`
- `shared/event-schemas.md`

## What Exists Before You

The Architect Agent has generated:

- `Ecommerce.Contracts.csproj`
- `Directory.Build.props`
- `Directory.Packages.props`

## What Comes After You

| Next Agent | How They Use Your Output |
|---|---|
| Common | References Contracts for base abstractions |
| Basket | Publishes `BasketCheckedOut` |
| Order | Consumes `BasketCheckedOut` and publishes order events |
| Logistics | Consumes shipment reservation events and publishes logistics events |
| Testing | Uses contracts to build cross-service scenarios |

## Versioning Rules

- V1 is the default consumed version
- V2 must remain additive over V1
- Transport concerns stay outside contract definitions
