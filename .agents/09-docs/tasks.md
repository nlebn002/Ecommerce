# Documentation - Tasks

## Task 1: Write `README.md`

**Description**: Provide a short project overview and a fast local start path.

**Must include**:

- what the solution is
- key services
- local run options
- where to find the dashboard and gateway

## Task 2: Write `architecture.md`

**Description**: Document service boundaries, infrastructure, and the canonical checkout flow.

**Must include**:

- service ownership
- event-driven communication
- gateway role
- storage boundaries

## Task 3: Write `api-reference.md`

**Description**: Document the public HTTP surface exposed through the gateway.

**Must include**:

- method
- route
- owning service
- example request and response

## Task 4: Write `events.md`

**Description**: Document all shared integration events from `Ecommerce.Contracts`.

**Must include**:

- event name
- producer
- consumer
- payload summary
- version notes where relevant

## Task 5: Write `local-setup.md`

**Description**: Document how to run the system locally with AppHost and with Docker Compose.

**Must include**:

- prerequisites
- startup commands
- verification steps
- test commands
