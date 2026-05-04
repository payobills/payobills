# SLM-Based Transaction Parsing

## Context

The current transaction parser uses hardcoded regex patterns per `BillType` (Amex, Jupiter, SBI-Prime, SavingsAccount, Testing). These are brittle — any change in SMS/notification format breaks parsing silently. This plan replaces the hardcoded regex system with an SLM running on a Raspberry Pi that generates regex patterns dynamically, which the Rust API then stores and reuses.

---

## Goal

- Deprovision the hardcoded regex system in `transaction_parser.rs`
- SLM on RPi becomes the source of truth for regex patterns
- Generated patterns are stored in NocoDB and reused for all subsequent transactions of that bill type
- Supervised learning (user feedback loop) is out of scope for this phase — tracked separately

---

## Architecture

```
n8n workflow
    │
    ▼
POST /api/parses/:id          ← Rust transaction-parser API
    │
    ├─ fetch transaction from NocoDB
    │
    ├─ look up stored regex for BillType
    │       │
    │       ├─ [regex exists] → apply regex → extract fields → PATCH NocoDB
    │       │
    │       └─ [no regex] → call SLM on RPi
    │                           │
    │                           ├─ SLM returns candidate regex
    │                           ├─ validate: compile + match against sample
    │                           ├─ [valid] store as Active in NocoDB
    │                           ├─ [invalid] retry once, then FailedNoPattern
    │                           └─ apply regex → extract fields → PATCH NocoDB
    │
    └─ currency normalization (unchanged)
```

---

## Components

### 1. SLM on Raspberry Pi

**Runtime:** Ollama (eventually vLLM) — exposed as a Kubernetes service inside the cluster. The Rust parser reaches it via a service endpoint, not a direct RPi IP.

**Model:** `qwen2.5-coder:1.5b` — small enough to run on RPi, strong enough for structured regex generation. (0.5B is likely too weak for reliable named-capture-group output.)

**Env var:** `SLM__BASE_URL` — set to the Kubernetes service URL (e.g. `http://ollama.inference.svc.cluster.local:11434`). Swapping Ollama for vLLM later only requires updating this value and adjusting the request format in `slm_client.rs`.

**Ollama endpoint used by Rust parser:**
```
POST {SLM__BASE_URL}/api/generate
```

Request body (Ollama format):
```json
{
  "model": "qwen2.5-coder:1.5b",
  "prompt": "<rendered prompt template>",
  "stream": false,
  "format": "json"
}
```

Response: Ollama wraps the model output in `{ "response": "..." }`. The `response` field should be a JSON string with:
```json
{
  "regex": "(?P<currency>[^\\d\\s]+)\\s*(?P<amount>[\\d,]+\\.?\\d+)...",
  "fields": ["currency", "amount", "merchant", "date"]
}
```

The `format: "json"` Ollama option constrains the model to output valid JSON. The Rust parser extracts `regex` from this.

**Note on vLLM migration:** vLLM exposes an OpenAI-compatible API (`/v1/chat/completions`), which is a different request shape than Ollama's `/api/generate`. The `slm_client.rs` module should be written with a thin abstraction (a trait or a backend enum) so the migration only touches one place.

---

### 2. Regex Store in NocoDB

New table: `transaction_regex_patterns`

| Column | Type | Notes |
|---|---|---|
| `BillType` | Text | Lookup key, matches `BillType` on transactions |
| `Pattern` | Long text | Rust `regex`-crate compatible pattern string |
| `Fields` | JSON | Named capture groups the pattern exposes |
| `GeneratedAt` | DateTime | When SLM produced it |
| `Status` | Text | `Active`, `Pending`, `Deprecated` |
| `SampleText` | Long text | The transaction text used to generate this pattern |

Only `Status = Active` patterns are used. `Pending` is reserved for the future supervised approval flow. The `SampleText` column is useful for debugging and re-generation.

**NocoDB env vars needed:**
```
NOCODB__TABLE_NAME__REGEX_PATTERNS=transaction_regex_patterns
```
(base and token are already present)

---

### 3. New Rust Modules

#### `src/payobills/regex_store.rs`

Responsible for:
- `get_active_pattern(nocodb_env, bill_type) -> Option<String>` — fetches `Active` pattern for a `BillType`
- `save_pattern(nocodb_env, bill_type, pattern, fields, sample_text)` — inserts a new row with `Status = Active`

Uses the existing `get_nocodb_records` helper pattern already established in `transaction_parser.rs`.

#### `src/payobills/slm_client.rs`

Responsible for:
- `generate_regex(slm_base_url, bill_type, sample_text) -> Result<String>` — calls Ollama, parses the response, returns the regex string
- Renders the prompt template (hardcoded in this module, not a file, to keep the binary self-contained)
- One retry on invalid output before returning `Err`

---

### 4. Changes to `transaction_parser.rs`

Replace the `if/else` BillType chain (lines 166–442) with:

```
1. get active pattern from regex_store for this BillType
2. if Some(pattern):
     apply regex to transaction_text
     if match: extract named captures → populate changes map
     if no match: set ParseStatus = FailedV1 (do NOT call SLM — bad input, not bad pattern)
3. if None:
     call slm_client::generate_regex(bill_type, transaction_text)
     if Ok(pattern):
       validate: compile the regex; test it matches transaction_text
       if valid: save to regex_store, apply, extract fields → changes map, ParseStatus = ParsedBySLM
       if invalid: set ParseStatus = FailedNoPattern
     if Err: set ParseStatus = FailedNoPattern (RPi unreachable → set NotStarted so it retries)
```

Named capture groups → NocoDB field mapping (the contract):

| Capture group | NocoDB field |
|---|---|
| `amount` | `Amount` |
| `merchant` | `Merchant` |
| `currency` | `Currency` |
| `date` | `BackDate` (fed through existing `parse_custom_date`) |

The `parse_custom_date` function will need a date format hint alongside the regex — this is a second field the SLM must return: `"date_format"`. See prompt design below.

Remove: all `BILL_TYPE_*` constants, the timezone map, and `parse_transaction`'s inner match block once the new path is working.

---

### 5. SLM Prompt Design

The prompt is rendered at runtime inside `slm_client.rs`. It must be precise about:
- Rust `regex` crate syntax (no lookaheads, no backreferences, named groups via `(?P<name>...)`)
- The exact JSON schema expected in the response
- That the pattern should generalise across transactions of the same type (vary amount/merchant/date, not the surrounding text)

Template:

```
You are a regex generation assistant. Generate a Rust `regex` crate compatible regular expression to extract fields from a bank transaction SMS.

Bill type: {bill_type}
Transaction text: "{sample_text}"

Extract these fields as named capture groups:
- amount (numeric, may include commas)
- merchant (business name, if present)
- currency (currency symbol or code, if present)
- date (date/time string as it appears, if present)

Rules:
- Use named capture groups: (?P<name>...)
- Do NOT use lookaheads, lookbehinds, or backreferences (not supported by Rust regex crate)
- The pattern must match the sample text above
- Make the pattern general enough to match other transactions of the same format (different amounts, merchants, dates)
- Omit capture groups for fields that are not present in this format

Respond with a JSON object only:
{
  "regex": "<pattern>",
  "fields": ["amount", ...],
  "date_format": "<strptime format string, or null if no date captured>"
}
```

---

## ParseStatus Values (revised)

| Status | Meaning |
|---|---|
| `NotStarted` | Not yet processed |
| `ParsedV1` | Parsed by stored regex |
| `ParsedBySLM` | No stored regex; SLM generated one and it was applied |
| `FailedV1` | Stored regex did not match (unexpected input format) |
| `FailedNoPattern` | No stored regex and SLM failed to generate a valid one |
| `ReParse` | Queued for retry (e.g. currency conversion failed) |

Note: `ParsedBySLM` and `ParsedV1` are effectively the same outcome — both used a regex. The distinction is useful only for observing how often the SLM path is hit.

---

## Implementation Sequence

These are ordered by dependency. Do not start a step until the previous is done and verified.

### Step 1 — RPi + Kubernetes: Deploy Ollama as a service

Install Ollama on the RPi and expose it via a Kubernetes service so the transaction-parser can reach it by a stable DNS name rather than a bare IP.

```bash
# on the RPi
curl -fsSL https://ollama.com/install.sh | sh
ollama pull qwen2.5-coder:1.5b
```

Create a Kubernetes `Service` (and `Endpoints` if the RPi is outside the cluster, or a node-port/externalName service if it's a cluster node) pointing at Ollama's port 11434. The exact manifest depends on how the RPi is registered in the cluster.

Verify reachability from inside the cluster (e.g. from a debug pod):
```bash
curl http://ollama.inference.svc.cluster.local:11434/api/generate \
  -d '{"model":"qwen2.5-coder:1.5b","prompt":"say hi","stream":false}'
```

Set `SLM__BASE_URL=http://ollama.inference.svc.cluster.local:11434` in the transaction-parser deployment env.

**Done when:** Ollama responds with a valid JSON response via the Kubernetes service DNS name.

---

### Step 2 — NocoDB: Create `transaction_regex_patterns` table

Manually create the table in NocoDB with the schema above. Add one test row for `BillType = Testing` with `Status = Active` and a known-good pattern to verify the lookup path independently.

**Done when:** a `GET` to NocoDB for `transaction_regex_patterns` returns the test row.

---

### Step 3 — Rust: Add `regex_store` module

New file: `src/payobills/regex_store.rs`

- `get_active_pattern` — GET with filter `(BillType,eq,X)~and(Status,eq,Active)`
- `save_pattern` — POST to NocoDB

Add `mod regex_store;` to `src/payobills/mod.rs`.

No changes to `transaction_parser.rs` yet.

**Done when:** a standalone test (or `cargo test`) can fetch the test row inserted in Step 2.

---

### Step 4 — Rust: Add `slm_client` module

New file: `src/payobills/slm_client.rs`

- `generate_regex(base_url, bill_type, sample_text) -> Result<GeneratedPattern>`
- `GeneratedPattern { regex: String, fields: Vec<String>, date_format: Option<String> }`
- Renders the prompt template, calls Ollama `/api/generate`, parses the `response` JSON field
- One retry on parse failure or invalid regex (compile check via `regex::Regex::new`)

Add `SLM__BASE_URL` to env var list in `api/parses.rs`.

**Done when:** calling `generate_regex` against the live RPi with a real Amex sample text returns a compilable regex that matches the input.

---

### Step 5 — Rust: Wire up new parse flow in `transaction_parser.rs`

Replace the `if/else` chain with the lookup → SLM fallback logic described in the Components section above.

Keep the old `if/else` block commented out (not deleted) until Step 6 passes — easy rollback.

**Done when:** a single transaction of each existing `BillType` parses correctly end-to-end via the new path.

---

### Step 6 — Pre-populate patterns for all existing BillTypes

Run the transaction-parser CLI (or a one-off script) against one real sample transaction per BillType to trigger SLM generation. Review the generated patterns in NocoDB before they are used in production — manually inspect and set `Status = Active` if they look correct.

Existing BillTypes to cover: `Amex`, `Jupiter`, `SBI-Prime`, `SavingsAccount`, `Testing`.

**Done when:** all five BillTypes have an `Active` pattern in `transaction_regex_patterns`.

---

### Step 7 — Remove hardcoded regex code

- Delete the `if/else` chain (lines 166–442 in current `transaction_parser.rs`)
- Delete `BILL_TYPE_*` constants
- Delete `timezone_to_offset` and `get_timezone_offset_map` (timezone handling moves into the SLM-generated `date_format` + `parse_custom_date`)
- Delete `TRANSACTION_DATE_FORMAT_*` constants

**Done when:** `cargo build` passes with no dead code warnings from the removed block.

---

### Step 8 — Smoke test full batch

Run `process_transactions` against all transactions with `ParseStatus = NotStarted` or `ReParse`. Verify:
- Transactions with known BillTypes use the stored regex path (`ParsedV1`)
- No transactions hit the SLM path (all patterns already stored from Step 6)
- Currency normalization still works

---

### Step 9 — Decommission n8n GenAI fallback step

Remove or disable the GenAI step in the n8n workflow. The Rust parser now handles both the fast path (stored regex) and the slow path (SLM generation).

---

## Open Questions

- **`date_format` reliability**: the SLM must return a strptime format string compatible with `parse_custom_date`. This needs careful prompt testing — wrong format strings will silently produce wrong dates.
- **SLM unavailability during parse**: if the Kubernetes service is unreachable and no pattern exists, should `ParseStatus` be `NotStarted` (silent retry) or `FailedNoPattern` (visible failure)? Leaning toward `NotStarted` to avoid false failures, but needs decision.
- **Ollama on RPi memory**: `qwen2.5-coder:1.5b` requires ~1GB RAM. RPi 4 (4GB) is fine; RPi 3 may be marginal under load.
- **Concurrent SLM calls**: the parser processes transactions sequentially today. If parallelised later, concurrent Ollama calls may degrade RPi performance or timeout.
- **vLLM migration**: vLLM uses an OpenAI-compatible API (`/v1/chat/completions`) — different request shape from Ollama. `slm_client.rs` should be written with a backend abstraction to make this a one-place change.

---

## Out of Scope (This Phase)

- Supervised learning / user feedback loop
- Pattern versioning / rollback UI
- Multi-sample regex generation
- SLM fine-tuning
- `Pending` → `Active` approval flow (table column exists but logic not wired)

---

## Future Problems

### Bank format changes

Bank SMS/notification formats rarely change, and even the hardcoded regex system had no handling for this. When a format does change, the stored `Active` pattern will start producing `FailedV1`. At that point, a manual trigger to regenerate the pattern for that `BillType` (deprecate the old pattern, let the SLM generate a new one on next parse) is sufficient. A more automated recovery path can be designed if format drift becomes a real operational problem.
