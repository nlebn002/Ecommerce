# Aspire Orchestrator - Tasks

## Task 1: Create `Ecommerce.ServiceDefaults`

**Description**: Create the shared host defaults used by every runnable project.

**Includes**:

- OpenTelemetry wiring
- health checks
- service discovery defaults
- shared resilience configuration

## Task 2: Create `Ecommerce.AppHost`

**Description**: Create the local distributed application host.

**Includes**:

- PostgreSQL resources
- RabbitMQ resource
- Redis resource when needed
- project registrations for Basket, Order, Logistics, and Gateway

## Task 3: Wire References

**Description**: Connect services to the infrastructure they depend on.

**Rules**:

- Basket, Order, and Logistics receive messaging and database references
- Gateway receives service project references
- Shared defaults are applied to every runnable service

## Task 4: Validate Local Developer Experience

**Description**: Ensure a developer can start the full system from AppHost and inspect it through the Aspire dashboard.

**Done When**:

- AppHost starts successfully
- all projects appear in the dashboard
- service dependencies are visible
- health and logs are available
