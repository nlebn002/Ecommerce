# Gateway - Tasks

## Task 1: Define Public Route Map

**Description**: Expose a stable HTTP surface for basket, order, and logistics capabilities through the gateway.

**Includes**:

- basket routes
- order routes
- logistics routes
- health endpoints

## Task 2: Configure Reverse Proxy

**Description**: Forward each public route to the owning service using YARP.

**Rules**:

- no business logic in the gateway
- preserve correlation headers
- support API versioning where required

## Task 3: Apply Edge Policies

**Description**: Configure gateway-only concerns.

**Includes**:

- rate limiting
- request logging
- health aggregation
- OpenAPI surface if exposed centrally

## Task 4: Support Checkout Flow

**Description**: Ensure the gateway surface supports the canonical event-driven checkout flow without introducing synchronous service-to-service orchestration.

## Task 5: Support Operational Endpoints

**Description**: Expose the endpoints needed for order lookup, cancellation, tracking, and diagnostics.
