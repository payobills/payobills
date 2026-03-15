# Payobills Agent

AI agent integration for the Payobills app. The agent interacts with the GraphQL gateway to query and mutate data on behalf of the user.

## Gateway

The gateway is a GraphQL Hive supergraph composed of the following subgraphs:
- **bills** — bills, bill statements, payments, stats
- **payments** — transactions, transaction tags
- **files** — file storage

GraphQL endpoint: `$GATEWAY_URL/graphql`

## Tools required in OpenWebUI

Two tools must be configured:

### `introspect_gateway()`
- Makes an introspection query to `$GATEWAY_URL`
- Returns the full schema — available queries, mutations, types, and arguments
- **Must be called before any other gateway call** to understand the schema

### `graphql_request(query, variables?)`
- Executes any GraphQL query or mutation against `$GATEWAY_URL`
- `query`: GraphQL operation string
- `variables`: optional dict of variables

Both tools read `GATEWAY_URL` from the environment. No auth is required — the gateway is VPN-protected.

## Prompts

See [`prompts/`](./prompts/) for ready-to-use prompts.

## Temporary agent progress

See [`AI/`](./AI/) for in-progress notes and scratchpad work.
