# Payobills Agent

AI agent integration for the Payobills app. The agent interacts with the GraphQL gateway to query and mutate data on behalf of the user.

## Gateway

The gateway is a GraphQL Hive supergraph composed of the following subgraphs:
- **bills** — bills, bill statements, payments, stats
- **payments** — transactions, transaction tags
- **files** — file storage

GraphQL endpoint: `$GATEWAY_URL/graphql`

## Direct GraphQL API Access

Direct GraphQL API calls to `$GATEWAY_URL/graphql` are the ONLY supported method. 

### GET Method Example

A standard practice is to use GET with the query encoded in the URL:

```
GET https://placeholder-gateway.example.com/graphql?query=query%20GetBills%20%7B%20billList%20%7B%20id%20name%20amount%20dueDate%20paid%20category%20%7D%20%7D
```

### Alternative POST Method

Or use POST with JSON body:

```json
POST https://placeholder-gateway.example.com/graphql
Content-Type: application/json
{
  "query": "{ billList { id name amount dueDate paid category }}"
}
```

No authentication is required — the gateway is VPN-protected.

## Prompts

See [`prompts/`](./prompts/) for ready-to-use prompts.

## Temporary agent progress

See [`AI/`](./AI/) for in-progress notes and scratchpad work.
