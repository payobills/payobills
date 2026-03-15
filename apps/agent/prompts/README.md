# Prompts Library

Ready-to-use prompts for the Payobills agent.

## Rules for all prompts

Every prompt must instruct the agent to:

1. **Always call `introspect_gateway()` first** — never assume the schema. Use the introspection result to discover the exact query names, argument names, and types before constructing any GraphQL operation.
2. Use `graphql_request(query, variables)` to execute operations.
3. Never hardcode field names or argument shapes — derive them from the introspection result.

## Available prompts

| File | Purpose |
|------|---------|
| [summarize-bills.md](./summarize-bills.md) | Fetch and summarize the user's bills |
