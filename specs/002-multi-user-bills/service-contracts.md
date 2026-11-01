# Service Contracts: Multiple Users and Shared Bills

## Authentication (bills, payments, files, user)

Clients send `Authorization: Bearer <access token>`.

A token is valid when its signature matches the issuer's published keys and its issuer, expiry and `azp` (UI client ID) are correct.

| Situation | `AUTH__ENFORCE=false` | `AUTH__ENFORCE=true` |
|---|---|---|
| Valid token | Token's user | Token's user |
| Invalid or expired token | 401 | 401 |
| No token | Allowed, no identity (auth slice only) | 401 |

Routes without authentication, needed for health probes and supergraph composition, are `GET /`, `GET /graphql?sdl` with `sdl` as the only parameter, and `GET /sdl` on user. They return no user data. `sdl` must be the only parameter because GET requests can execute queries.

A service refuses to start without `AUTH__ISSUER` and `AUTH__CLIENT_ID`, and enforces when `AUTH__ENFORCE` is unset.

### Configuration

| Variable | Used by | Purpose |
|---|---|---|
| `AUTH__ISSUER` | bills, payments, files, user | Expected token issuer |
| `AUTH__CLIENT_ID` | bills, payments, files, user | Expected `azp` |
| `AUTH__ENFORCE` | bills, payments, files, user | Refuse requests with no token |
| `FIRST_USER_SUBJECT` | backfill script | Owner of all existing records |
| `IMPORT_OWNER_SUBJECT` | transaction-syncer | Owner of imported transactions |

## Gateway

Must forward the `Authorization` header to all subgraphs. Its configuration lives outside this repo.

## GraphQL

### User subgraph

```graphql
type User @key(fields: "id") {
  id: ID!            # token subject
  email: String
  displayName: String
  createdAt: DateTimeISO!
  updatedAt: DateTimeISO!
}

type Query {
  me: MeResponse!    # creates the account on first call
}
```

`email` is returned only for the caller and for people sharing a bill with them.

### Bills subgraph

```graphql
enum BillRole { OWNER MEMBER }

type BillMember {
  user: User!
  role: BillRole!
  createdAt: DateTimeISO!
}

extend type Bill {
  myRole: BillRole!
  members: [BillMember!]!
  receipts: [File!]!   # receipts with no bill cycle
}

extend type Payment { createdBy: User }
extend type BillStatement { createdBy: User }

type Mutation {
  billMemberAdd(billId: ID!, email: String!): BillMember!   # owner only
  billMemberRemove(billId: ID!, userId: ID!): Boolean!      # owner only
  billLeave(billId: ID!): Boolean!                          # member only
}
```

Bills needs an email-to-account lookup. It reads the `users` table directly, the same way services already share the NocoDB base, instead of adding a service-to-service call.

### Errors

| Code | When |
|---|---|
| `UNAUTHENTICATED` | Missing or invalid identity |
| `NOT_FOUND` | Record does not exist or the caller may not see it |
| `FORBIDDEN` | The caller can see the bill but lacks the role for this action |
| `USER_NOT_FOUND` | No account for this email. The person must sign in once first |
| `ALREADY_MEMBER` | The person is already on this bill |
| `OWNER_CANNOT_LEAVE` | An owner tried to leave their own bill |

### Scoped operations (signature unchanged)

| Service | Operations | Rule |
|---|---|---|
| Bills | `bills`, `billById`, `billStats`, `billStatements`, `billStatementById`, `addOrUpdateBillStatement`, `markPayment` | Caller is a member of the bill |
| Bills | `addBill` | Creates the bill and the caller's owner row |
| Payments | `transactions`, `transactionByID`, `transactionsByYearAndMonth`, `transactionStats`, `transactionUpdate`, `setTransactionTags`, `transactionReceiptsSync` | Caller owns the transaction or is a member of its bill |
| Payments | `transactionAdd` | Caller becomes owner. A linked bill requires membership |
| Payments | `transactionTags`, `transactionTagsAddOrUpdate`, `trips`, `tripCreate`, `tripUpdate` | Caller's own records only |
| Files | `files`, `File` entity | See REST rules below |

## Files REST

| Route | Rule |
|---|---|
| `POST /files` | Sets owner from token. `TransactionID` or bill `CorrelationID` tags must name a record the caller can access, otherwise 404. Type `BILL_RECEIPT` stores the bill in `BillId` and needs no bill cycle |
| `GET /files/:id` | Uploader, or anyone with access to a linked transaction, statement or bill, otherwise 404 |
| `DELETE /files/:id` | Same rule as GET |

## Events

No changes to RabbitMQ message formats.
