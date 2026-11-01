# Feature Specification: Multiple Users and Shared Bills

**Feature Branch**: `feat-multi-user-bills`

**Created**: 2026-10-31

**Status**: Draft

**Input**: User description: "Payobills must support multiple users, each signing in and seeing only their own data, and a bill can be shared by multiple users."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Each person has private data (Priority: P1)

A person signs in and sees only their own bills, statements, payments, transactions, trips and files. Nobody can read or change another person's data, whether through the app or by calling a service directly.

**Why this priority**: Nothing else is safe without it. This is financial data, and today nothing separates one person's records from another's.

**Independent Test**: Sign in as two people, create a bill and a transaction as each, and confirm neither can see or open the other's, including by direct link and by request without a sign-in.

**Acceptance Scenarios**:

1. **Given** two signed-in people with separate data, **When** either views any list, total or statistic, **Then** only their own data is included.
2. **Given** a person who knows the ID of another person's record, **When** they request or change it, **Then** the request is refused the same way as for a record that does not exist.
3. **Given** a request with no valid sign-in, **When** it reaches any user-facing service, **Then** it is refused and no data is returned.

---

### User Story 2 - Existing data stays with its owner (Priority: P1)

The current single user upgrades and finds all their existing data under their account, and automatic transaction imports keep working.

**Why this priority**: Losing or hiding years of existing records would make the upgrade unacceptable.

**Independent Test**: Run the upgrade against a copy of production data and compare record counts and totals for the original owner before and after.

**Acceptance Scenarios**:

1. **Given** existing records with no owner, **When** the upgrade completes, **Then** every record belongs to the designated first user and their totals are unchanged.
2. **Given** the upgrade has completed, **When** a transaction is imported automatically, **Then** it belongs to the designated import owner.
3. **Given** a second person signs in for the first time, **When** they open the app, **Then** they see an empty account and none of the first user's data.

---

### User Story 3 - Share a bill (Priority: P2)

A bill owner adds another person to a bill, for example household rent. Both then see that bill with its statements, payments and linked transactions, and either can record a payment.

**Why this priority**: This is the collaboration the feature exists for, but it depends on User Story 1.

**Independent Test**: As the owner, share one of two bills with a second person. Confirm they see only that bill, and that a payment recorded by either person appears for both.

**Acceptance Scenarios**:

1. **Given** an owner and a second person who has signed in before, **When** the owner adds them by email, **Then** the bill and its related records appear for that person.
2. **Given** a shared bill, **When** any member records a payment or adds a statement, **Then** all members see it and it shows who recorded it.
3. **Given** a shared bill, **When** a member views their other data, **Then** the owner's unshared bills and transactions remain hidden.
4. **Given** an email that does not match anyone who has signed in, **When** the owner tries to add it, **Then** nobody is added and the owner is told that person must sign in once first.

---

### User Story 4 - Stop sharing (Priority: P3)

An owner removes a member, or a member leaves a bill, and access ends.

**Why this priority**: Sharing needs an exit, but it is rarely used compared with sharing itself.

**Independent Test**: Remove a member, then confirm the bill and its files are no longer reachable by them, including by direct link.

**Acceptance Scenarios**:

1. **Given** a member of a shared bill, **When** the owner removes them or they leave, **Then** they lose access to the bill and its related records on their next request.
2. **Given** a removed member recorded payments, **When** remaining members view the bill, **Then** those payments remain.
3. **Given** a bill owner, **When** they try to leave their own bill, **Then** this is refused.

---

### User Story 5 - Add a receipt without choosing a bill cycle (Priority: P3)

A person adds a receipt to a bill straight from the bill page, without picking a billing period or finding a transaction first.

**Why this priority**: It removes friction from everyday use, but is independent of the access work.

**Independent Test**: Open a bill, add a receipt with no period chosen, and confirm it is listed on the bill and can be opened.

**Acceptance Scenarios**:

1. **Given** a bill, **When** a person adds a receipt, **Then** they are not asked for a bill cycle and the receipt appears on that bill.
2. **Given** a receipt on a shared bill, **When** any member opens the bill, **Then** they can open the receipt.

### Edge Cases

- A session expires mid-use: the person is asked to sign in again and unsaved form input is not reported as saved.
- An owner adds someone who is already a member: no duplicate is created and the owner is told.
- A person's account is disabled at the identity provider: all their access ends when their current session expires, within 15 minutes.
- Two people share a device: after sign-out, the previous person's data and files are not available offline.
- A transaction is moved to another bill: visibility follows the new bill.
- A non-member opens a direct link to a receipt or statement file: it is refused.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Every user-facing service MUST reject requests without a valid signed-in identity.
- **FR-002**: Each person MUST have an account, created automatically on first sign-in.
- **FR-003**: Each bill, transaction, transaction tag, trip and uploaded file MUST have exactly one owner, set to the person who created it.
- **FR-004**: Every read, write, list, total and statistic MUST include only records the person may access.
- **FR-005**: A person's access MUST be determined by their verified identity and never by an identifier the client supplies.
- **FR-006**: Bill owners MUST be able to add members by email, limited to people who have signed in at least once.
- **FR-007**: Owners MUST be able to list and remove members, and members MUST be able to leave.
- **FR-008**: Members MUST be able to view the bill and its statements, payments, linked transactions and files, and to add or update them.
- **FR-009**: Only the owner MUST be able to change a bill's membership.
- **FR-010**: Payments and statements MUST record who created them, and the record MUST survive that person leaving.
- **FR-011**: Files MUST be accessible only to people who can access the record they are attached to, or to their uploader if unattached.
- **FR-012**: All existing records MUST be assigned to a designated first user during upgrade without changing their contents.
- **FR-013**: Automatically imported records MUST be assigned to a configured import owner.
- **FR-014**: Signing out MUST clear the person's cached data and files from that device.
- **FR-015**: The upgrade MUST be possible without downtime for the first user, with enforcement switched on as a separate step.
- **FR-016**: People MUST be able to add a receipt to a bill without choosing a bill cycle or a transaction.
- **FR-017**: Pages other than sign-in MUST require a signed-in person.

### Key Entities

- **User**: A person with an account, identified by their identity-provider subject, with an email and display name.
- **Bill Membership**: Links a user to a bill with the role owner or member. Each bill has exactly one owner.
- **Owned records**: Bills, transactions, transaction tags, trips and files gain an owner. Statements and payments inherit access from their bill, and bill-linked transactions are also visible to that bill's members.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of attempts to reach another person's records or files are refused in an access test covering every query, mutation and file route.
- **SC-002**: After upgrade, the first user's record counts and totals match the pre-upgrade values exactly.
- **SC-003**: An owner can share a bill in under one minute.
- **SC-004**: A removed member loses access on their next request, with no waiting period.
- **SC-005**: Existing screens remain usable with no perceptible slowdown for a person with up to 50 bills and 10,000 transactions.

## Assumptions

- **Sharing means shared access, not splitting costs.** Every member sees full amounts. Who owes what is out of scope.
- **Two roles only.** Owner and member differ only in managing membership. Transferring ownership and deleting bills are out of scope.
- **Personal transactions stay private.** Only transactions linked to a shared bill are visible to its members, and totals show full amounts for each member.
- **Accounts come from the existing identity provider.** Sign-up, passwords and account administration stay there, with no new registration screen.
- **Imports stay single-person.** Automatic imports go to one configured owner. Other people add transactions manually until per-person imports are specified separately.
- **Adding by email can reveal that a person has an account.** This is acceptable for a private, self-hosted instance with known users.
- **Cycle-free receipts belong to a bill.** They are not matched to a transaction or a billing period automatically.
- **Direct database access is trusted.** Anyone with administrative access to the data store can still see all data.
