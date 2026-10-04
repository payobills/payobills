# Service Contracts: Bill Types

**Date**: 2026-06-11 | **Plan**: [plan.md](plan.md)

The `bills` service exposes a HotChocolate GraphQL API (Apollo Federation compatible).

## `addBill` Mutation — Input Change

`billingPeriod` is added as an optional field. Allowed values: `"monthly"`, `"yearly"`, `"prepaid"`. Defaults to `"monthly"` when omitted.

```graphql
input CreateBillDtoInput {
  name: String!
  billingDate: Int
  payByDate: Int
  latePayByDate: Int
  primaryType: String
  billingPeriod: String   # NEW — allowed: "monthly" | "yearly" | "prepaid"; default "monthly"
}
```

## `addBill` Mutation — Return Type (unchanged)

`BillDto` already includes `billingPeriod`. No change to the return shape.

```graphql
type BillDto {
  id: String!
  name: String!
  billingDate: Int
  billingPeriod: String   # already present
  payByDate: Int
  latePayByDate: Int
  isEnabled: Boolean!
  primaryType: String
  createdAt: DateTime!
  updatedAt: DateTime!
  payments: [PaymentDto!]!
}
```

## `bills` and `billById` Queries (unchanged)

Both queries already return `billingPeriod` via `BillDto`. UI clients must add `billingPeriod` to their selection sets.

### UI `bills` query — updated selection set

```graphql
query {
  bills {
    id
    name
    billingDate
    payByDate
    billingPeriod   # ADD THIS
    isEnabled
    createdAt
    updatedAt
  }
}
```

### UI `addBill` mutation — updated

```graphql
mutation ($name: String!, $payByDate: Int, $billingDate: Int, $billingPeriod: String) {
  addBill(billDto: {
    name: $name
    payByDate: $payByDate
    billingDate: $billingDate
    latePayByDate: 0
    billingPeriod: $billingPeriod
  }) {
    name
    payByDate
    billingDate
    billingPeriod
    createdAt
    updatedAt
    id
  }
}
```
