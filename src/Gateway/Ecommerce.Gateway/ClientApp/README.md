# Ecommerce Console Client

Angular frontend for the gateway-hosted operator console.

## Run

```bash
npm install
npm start
```

`npm start` serves the Angular app on `http://localhost:4200` and proxies API calls to the gateway at `http://localhost:5010`.

## Build for the gateway

```bash
npm run build:gateway
```

This writes the production build into `../wwwroot/console`, which the ASP.NET gateway serves at `/console`.

## Contracts

The app currently uses local TypeScript interfaces that mirror the backend DTOs. When the gateway is running, refresh generated contracts with:

```bash
npm run generate:contracts
```

Generated files land in `src/app/core/models/generated/` and are intended to replace the handwritten DTO mirrors over time.
