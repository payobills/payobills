# PayoBills Transaction Parser

Parse Notion transaction data from transaction text to meaningful transaction payobills records.

## Changelog

- 0.3.2: Also parse ReParse Type of transactions along with NotStarted ones.

## Architecture

```
┌─────────────────────┐
│    CLI/API Server    │
├─────────────────────┤
│ Main: transaction-parser │
├─────────────────────┤
│ API: /health check   │
│      /api/parses/:id │
├─────────────────────┤
│ Core: transaction_parser.rs │
└─────────────────────┘
```

## Core Features

- **4 Bill Types**: Amex, Jupiter, SBI Prime, UPI Savings Account
- **Regex-based parsing**: Extracts merchant, amount, date
- **Currency conversion**: Via NocoDB API with exchange rates
- **Multi-format date parsing**: chrono + jiff support
- **Timezone handling**: IST, PST, EST, GMT, UTC, etc.

## HTTP Endpoints

| Method | Path                                      | Description                  |
|--------|-------------------------------------------|------------------------------|
| GET    | /                                         | Health check                 |
| POST   | /api/parses/:transaction_id              | Parse single transaction     |

## CLI

Process all pending `ParseStatus=ReParse` or `NotStarted` transactions in batch mode.

## Environment Variables

```env
SERVER_ADDRESS           # Port to bind (default: 0.0.0.0:31004)
NOCODB__BASE_URL         # NocoDB API endpoint
NOCODB__INTEGRATION_TOKEN # API key
NOCODB__BASE_NAME__CURRENCIES   # Currency DB base
NOCODB__TABLE_NAME__CURRENCIES__HISTORICAL   # Exchange rates table
NOCODB__TABLE_NAME__CURRENCIES__CURRENCIES   # Currency codes table
NOCODB__TABLE_NAME     # Transactions table
RUST_BACKTRACE          # Set to "full" for detailed error traces
```

## Dependencies

- **Core**: serde, chrono, regex, reqwest
- **HTTP**: axum, clap, notiom, isahc
- **Time**: jiff, chrono
- **Test**: httpmock, mockito

## Deployment

- **Kubernetes**: Helm chart with NocoDB integration
- **Docker**: Multi-platform (linux/arm64,linux/amd64)
- **Registry**: `ghcr.io/payobills/tools/transaction-parser`

## Build

```bash
# Production build
RELEASE=true cargo build --release

# Test
cargo test

# Run API server
cargo run --bin transaction-parser -- api

# Run CLI batch processing
cargo run --bin transaction-parser -- cli
```
