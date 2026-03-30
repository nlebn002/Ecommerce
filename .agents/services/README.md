# Service Creation Guides

This folder turns the Basket service into a repeatable template for the next services.

Use these files in this order:

1. `service-template-rules.md`
2. `order-service.md`
3. `logistics-service.md`

Important:

- The canonical Basket implementation in this repository is `src/Services/Basket/Ecommerce.BasketService.*`
- Do not copy the older `Ecommerce.Basket.*` projects
- Keep the same layered structure and infrastructure conventions unless there is a clear repo-wide reason to change them
