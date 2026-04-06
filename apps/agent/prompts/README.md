# Prompts Library

Ready-to-use prompts for the Payobills agent.

## Rules for all prompts

Prompts should construct direct GraphQL queries/mutations for the gateway endpoint:

1. Start with a standard GraphQL introspection query: `query IntrospectionQuery { __schema { ... } }`
2. Use direct POST requests to `$GATEWAY_URL/graphql` with JSON payloads.
3. Never hardcode field names — derive them from introspection results.

## Available prompts

| File | Purpose |
|------|---------|
| [summarize-bills.md](./summarize-bills.md) | Fetch and summarize the user's bills |
