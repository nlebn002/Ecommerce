# Gateway

## Purpose

`src/Gateway/Ecommerce.Gateway` is the public HTTP edge for the solution. It provides:

- reverse proxy routing to the underlying services
- a unified OpenAPI and Scalar experience
- static hosting for the local demo console
- request rate limiting
- health and metrics endpoints

The gateway does not own business data. Its responsibility is traffic management and API discoverability.

## Routing

The gateway uses YARP and service discovery. Routes are defined in `appsettings.json`.

Supported public prefixes:

- `/basket/...`
- `/orders/...`
- `/logistics/...`

Compatibility-style direct prefixes also exist:

- `/api/basket/...`
- `/api/orders/...`
- `/api/logistics/...`

Each route rewrites to the target service's `/api/...` path.

Examples:

- `/basket/v1/baskets` -> basket service `/api/v1/baskets`
- `/orders/v1/orders/{orderId}` -> order service `/api/v1/orders/{orderId}`
- `/logistics/v1/shipments/by-order/{orderId}` -> logistics service `/api/v1/shipments/by-order/{orderId}`

In Compose, cluster destinations are overridden in `appsettings.Compose.json` to explicit `http://{service-name}:8080/` addresses.

## OpenAPI and Scalar

The gateway exposes:

- `/openapi/v1.json` for the gateway itself
- `/scalar` for the aggregated API reference UI

It also proxies each downstream OpenAPI document through:

- `/scalar-docs/basket/v1.json`
- `/scalar-docs/order/v1.json`
- `/scalar-docs/logistics/v1.json`

When the gateway fetches a service OpenAPI document, it rewrites:

- `paths` so service-local `/api/...` routes appear under gateway prefixes like `/orders/...`
- `servers` so generated requests target the gateway rather than the backing service directly

This keeps the documentation aligned with how clients are expected to call the system.

## Demo Console

The gateway serves static files from `wwwroot` and redirects:

- `/console` -> `/console/index.html`

The console is a lightweight browser client for exercising the solution through the gateway.

## Rate Limiting

The gateway applies a fixed-window limiter named `gateway` to proxied traffic:

- `100` requests
- per `10` seconds
- no queueing

The limiter is attached to `MapReverseProxy()`, which means business traffic through the gateway is throttled centrally.

## Health and Metrics

Because the gateway uses `AddServiceDefaults()` and `MapDefaultEndpoints()`, it exposes:

- `/health/live`
- `/health/ready`
- `/metrics`

These endpoints integrate with the same observability model as the business services.

## Root Endpoint

The root path returns a simple status payload:

```json
{ "service": "gateway", "status": "ok" }
```

This is useful as a quick readiness check from outside the stack.
